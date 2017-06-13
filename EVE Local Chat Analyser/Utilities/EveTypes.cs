using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EveLocalChatAnalyser.Utilities
{
    public class EveTypes : IEveTypes
    {
        private readonly ITypeLoader _loader;
        private readonly ITypeStorage _storage;
        private HashSet<string> _names;
        private Dictionary<int, string> _namesById;

        public EveTypes(ITypeStorage storage, ITypeLoader loader)
        {
            //TODO race condition on initial loading
            _storage = storage;
            _loader = loader;
            LoadTypes();
            var isFirst = !_names.Any();
            Task.Factory.StartNew(() => Update(isFirst), TaskCreationOptions.LongRunning);
        }

        public bool IsShipTypeName(string name)
        {
            EnsureInitialLoad();
            return _names.Contains(name);
        }

        public bool IsPod(int shipTypeID)
        {
            return shipTypeID == 670 || shipTypeID == 33328;
        }

        public string this[int typeId]
        {
            get
            {
                EnsureInitialLoad();
                string name;
                return _namesById.TryGetValue(typeId, out name) ? name : ("unknown: " + typeId);
            }
        }

        private async Task Update(bool isFirst)
        {
            var types = (await _loader.LoadShipTypes()
                                   .ConfigureAwait(true)).ToArray();

            if (_names.Count != types.Length)
            {
                try
                {
                    InitTypes(types);
                    _storage.SetTypes(types);
                }
                finally
                {
                    if (isFirst)
                    {
                        lock (this)
                        {
                            Monitor.PulseAll(this);
                        }
                    }
                }
            }
        }

        private void LoadTypes()
        {
            var types = _storage.ShipTypes.ToArray();
            InitTypes(types);
        }

        private void InitTypes(TypeInfo[] types)
        {
            Interlocked.Exchange(ref _namesById, types.ToDictionary(x => x.TypeID, x => x.Name));
            Interlocked.Exchange(ref _names, new HashSet<string>(types.Select(x => x.Name)));
        }

        private void EnsureInitialLoad()
        {
            lock (this)
            {
                if (!_names.Any())
                {
                    Monitor.Wait(this);
                }
            }
        }
    }
}