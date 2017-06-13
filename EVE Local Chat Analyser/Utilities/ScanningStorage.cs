using System;
using System.Collections.Generic;
using System.Linq;
using EveLocalChatAnalyser.Utilities.PositionTracking;
using LiteDB;

namespace EveLocalChatAnalyser.Utilities
{
    public class ScannedSignature
    {
        public string Id
        {
            get { return System + "/" + Name; }
            set { }
        }

        //indicates wether on the last scan copy that signature was no longer present
        //this could result from it being closed or the user just copied e.g. a single other signature to
        //the clipboard. In the latter case, we don't want to throw it away, if we already scanned it.
        public bool IsPotentiallyClosed { get; set; }

        public string System { get; set; }

        public string Name { get; set; }

        public string Group { get; set; }

        public string Type { get; set; }

        public DateTime ScanTime { get; set; }

        public virtual string ScanTimeStr
        {
            get { return ScanTime.ToString("yyyy-MM-dd HH:mm"); }
        }

        public bool IsWormhole()
        {
            return Group == "Wormhole";
        }

        public bool IsUnknown()
        {
            return string.IsNullOrEmpty(Group);
        }

        protected bool Equals(ScannedSignature other)
        {
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((ScannedSignature) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public delegate void ScannedSignaturesUpdate(string system, IEnumerable<ScannedSignature> scanItems);

    //TODO als service anbieten
    public static class ScanningStorage
    {
        private static readonly IPositionTracker POSITION_TRACKER = DIContainer.GetInstance<IPositionTracker>();
        //TODO per DI als service

        private static readonly TimeSpan MAX_VALIDITY = new TimeSpan(5, 0, 0, 0);

        public static event ScannedSignaturesUpdate ScannedSignaturesUpdate;

        private static void OnScannedSignaturesUpdate(string system, IEnumerable<ScannedSignature> scanitems)
        {
            var handler = ScannedSignaturesUpdate;
            if (handler != null)
            {
                handler(system, scanitems);
            }
        }

        //TODO scanning storage ueber DI
        public static void OnProbeScan(IList<IProbeScanItem> scanItems)
        {
            //TODO es sollte n event fuer store update geben, dann koennte das asynchron gemacht werden hier ...
            var system = POSITION_TRACKER.CurrentSystemOfActiveCharacter;
            if (system == null)
            {
                return;
            }

            var now = DateTime.UtcNow;

            var signaturesSighted = GetScannedSignaturesAtCurrentlyActivePosition();
            var signaturesOnScan = scanItems.Where(x => x.IsCosmicSignature)
                .Select(
                        x => new ScannedSignature
                             {
                                 System = system,
                                 Name = x.Name,
                                 Group = x.Group,
                                 Type = x.Type,
                                 ScanTime = now
                             })
                .ToArray();

            var newOrImprovedItems = (from sig in signaturesOnScan
                                      let knownSig = signaturesSighted.FirstOrDefault(ka => ka.Name == sig.Name)
                                      where
                                          knownSig == null
                                          || (string.IsNullOrWhiteSpace(knownSig.Group) && !string.IsNullOrWhiteSpace(sig.Group))
                                          || (string.IsNullOrWhiteSpace(knownSig.Type) && !string.IsNullOrWhiteSpace(sig.Type))
                                      select new ScannedSignature
                                             {
                                                 System = system,
                                                 Name = sig.Name,
                                                 Group = sig.Group,
                                                 Type = sig.Type,
                                                 ScanTime = knownSig == null ? now : knownSig.ScanTime
                                             }).ToArray();

            var reopened = signaturesSighted.Where(x => x.IsPotentiallyClosed && signaturesOnScan.Contains(x))
                .ToArray();
            var potentiallyClosed = signaturesSighted.Except(signaturesOnScan)
                .ToArray();

            UpdateDatabase(newOrImprovedItems, system, potentiallyClosed, reopened);

            var bestSignatureInfoOnScan = signaturesSighted.Intersect(signaturesOnScan)
                .Except(newOrImprovedItems)
                .Concat(newOrImprovedItems)
                .ToArray();
            OnScannedSignaturesUpdate(system, bestSignatureInfoOnScan);
        }

        private static void UpdateDatabase(ScannedSignature[] scannedItems,
                                           string system,
                                           ScannedSignature[] potentiallyClosed,
                                           ScannedSignature[] reopened)
        {
            using (var session = App.CreateStorageEngine())
            {
                var collection = session.GetCollection<ScannedSignature>();
                UpdateNewAndImprovedScanResults(scannedItems, system, collection);

                UpdatePotentiallyClosed(system, potentiallyClosed, collection, false);

                UpdatePotentiallyClosed(system, reopened, collection, true);
            }
        }

        private static void UpdatePotentiallyClosed(string system,
                                                    ScannedSignature[] reopened,
                                                    Collection<ScannedSignature> session,
                                                    bool IsPotentiallyClosed)
        {
            foreach (var curReopened in reopened)
            {
                var sig = session.FindById(curReopened.Id);
                sig.IsPotentiallyClosed = IsPotentiallyClosed;
                session.Update(sig);
            }
        }

        private static void UpdateNewAndImprovedScanResults(ScannedSignature[] scannedItems,
                                                            string system,
                                                            Collection<ScannedSignature> session)
        {
            foreach (var curItem in scannedItems)
            {
                var sig = session.FindById(curItem.Id);
                if (sig == null)
                {
                    session.Insert(curItem);
                }
                else
                {
                    sig.Group = curItem.Group;
                    sig.Type = curItem.Type;
                    session.Update(sig);
                }
            }
        }

        public static ICollection<ScannedSignature> GetScannedSignaturesAtCurrentlyActivePosition()
        {
            var system = POSITION_TRACKER.CurrentSystemOfActiveCharacter;
            if (system == null)
            {
                //TODO  ...
                return new List<ScannedSignature>();
            }

            return GetScannedSignaturesForSystem(system);
        }

        public static ICollection<ScannedSignature> GetScannedSignaturesForSystem(string system)
        {
            using (var session = App.CreateStorageEngine())
            {
                var collection = session.GetCollection<ScannedSignature>();
                var now = DateTime.UtcNow;
                var signatures = collection.Find(x => x.System == system)
                    .ToArray();
                //TODO wormholes schon nach 48h rausschmeissen (und mit wh connection koppeln oder so)
                var outdatedSignatures = signatures.Where(x => (now - x.ScanTime) > MAX_VALIDITY)
                    .ToList();
                foreach (var outdatedSignature in outdatedSignatures)
                {
                    collection.Delete(outdatedSignature.Id);
                }
                return signatures.Except(outdatedSignatures)
                    .OrderBy(x => x.Name)
                    .ToArray();
            }
        }
    }
}
