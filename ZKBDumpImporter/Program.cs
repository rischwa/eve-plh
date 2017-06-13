using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PLHLib;

namespace ZKBDumpImporter
{
    public abstract class BaseDataReader : IDataReader
    {
        public string GetName(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public Type GetFieldType(int i)
        {
            throw new NotImplementedException();
        }

        public abstract object GetValue(int i);
        public abstract int GetOrdinal(string name);

        public bool GetBoolean(int i)
        {
            throw new NotImplementedException();
        }

        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            throw new NotImplementedException();
        }

        public int GetInt32(int i)
        {
            throw new NotImplementedException();
        }

        public long GetInt64(int i)
        {
            throw new NotImplementedException();
        }

        public float GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        public string GetString(int i)
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            throw new NotImplementedException();
        }

        public abstract int FieldCount { get; }

        object IDataRecord.this[int i]
        {
            get { throw new NotImplementedException(); }
        }

        object IDataRecord.this[string name]
        {
            get { throw new NotImplementedException(); }
        }


        public abstract void Close();

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public bool NextResult()
        {
            throw new NotImplementedException();
        }

        public abstract bool Read();

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }


        public int Depth { get; private set; }
        public bool IsClosed { get; private set; }
        public int RecordsAffected { get; private set; }
        public abstract void Dispose();
    }

    public abstract class AbstractSqlBulkDataReader<T> : BaseDataReader
    {
        protected IEnumerator<T> _enumerator;

        public AbstractSqlBulkDataReader(IEnumerable<T> values)
        {
            _enumerator = values.GetEnumerator();
        }


        public override void Close()
        {
            _enumerator.Dispose();
            _enumerator = null;
        }

        public override bool Read()
        {
            return _enumerator.MoveNext();
        }

        public override void Dispose()
        {
            if (_enumerator != null)
            {
                _enumerator.Dispose();
                _enumerator = null;
            }
        }
    }

    public class KillDataReader : AbstractSqlBulkDataReader<Kill>
    {
        public KillDataReader(IEnumerable<Kill> kills) : base(kills)
        {
        }

        public override int FieldCount
        {
            get { return 4; }
        }

        public override object GetValue(int i)
        {
            switch (i)
            {
                case 0:
                    return _enumerator.Current.KillID;
                case 1:
                    return _enumerator.Current.SolarSystemID;
                case 2:
                    return _enumerator.Current.KillTime;
                case 3:
                    return _enumerator.Current.MoonID;
                default:
                    throw new ArgumentException("no such index in kill row: " + i);
            }
        }


        public override int GetOrdinal(string name)
        {
            switch (name)
            {
                case "KillID":
                    return 0;
                case "SolarSystemID":
                    return 1;
                case "KillTime":
                    return 2;
                case "MoonID":
                    return 3;
                default:
                    throw new ArgumentException("no such column name in kill row: " + name);
            }
        }
    }

    public abstract class NestedDataReader<T, K> : BaseDataReader
    {
        private readonly Func<T, IEnumerator<K>> _getChildEnumerator;
        protected IEnumerator<K> _childEnumerator;
        protected Func<K, object>[] _mappings;
        protected Dictionary<string, int> _nameMappings;
        protected IEnumerator<T> _parentEnumerator;

        protected NestedDataReader(IEnumerable<T> parentValues, Func<T, IEnumerator<K>> getChildEnumerator,
                                   params Expression<Func<K, object>>[] mappings)
        {
            _getChildEnumerator = getChildEnumerator;
            _parentEnumerator = parentValues.GetEnumerator();
            _mappings = mappings.Select(x => x.Compile()).ToArray();
            //SwitchExpression switchExpr =
            //    Expression.Switch(Expression.Parameter(typeof (string)), 
            //    mappings.Select(x=>((MemberExpression)x.Body).Member.Name).Select(x=>Expression.SwitchCase(Expression.Block(Expression.Constant(x)))).ToArray()
            //        );
            var memberNames = mappings.Select(x=>(x.Body as MemberExpression ?? ((UnaryExpression) x.Body).Operand as MemberExpression).Member.Name).ToArray();
            _nameMappings = new Dictionary<string, int>();
            for (var i = 0; i < mappings.Length; ++i)
            {
                _nameMappings[memberNames[i]] = i;
            }
        }

        public void AddMappings(SqlBulkCopy bulkCopy)
        {
            foreach (var curName in _nameMappings.Keys)
            {
                bulkCopy.ColumnMappings.Add(curName, curName);
            }
        }

        public override void Close()
        {
            Dispose();
        }

        public override bool Read()
        {
            if (_childEnumerator == null || !_childEnumerator.MoveNext())
            {
                if (!_parentEnumerator.MoveNext())
                {
                    return false;
                }
                _childEnumerator = _getChildEnumerator(_parentEnumerator.Current);
                return Read();
            }
            return true;
        }

        public override void Dispose()
        {
            if (_parentEnumerator != null)
            {
                _parentEnumerator.Dispose();
                _parentEnumerator = null;
            }

            if (_childEnumerator != null)
            {
                _childEnumerator.Dispose();
                _childEnumerator = null;
            }
        }


        public override int GetOrdinal(string name)
        {
            return _nameMappings[name];
        }
    }

    public class ItemDataReader : NestedKillDataReader<Item>
    {
        public ItemDataReader(IEnumerable<Kill> values)
            : base(
                values, kill => kill.Items.GetEnumerator(), item => item.TypeID, item => item.QtyDestroyed,
                item => item.QtyDropped, item => item.Singleton, item => item.Flag)
        {
        }
    }

    //TODO killid kann auch noch in ne oberklasse gezogen werden
    public class AttackerDataReader : NestedKillDataReader<Attacker>
    {
        public AttackerDataReader(IEnumerable<Kill> parentValues)
            : base(
                parentValues, kill => kill.Attackers.GetEnumerator(), attacker => attacker.SecurityStatus,
                attacker => attacker.ShipTypeID, attacker => attacker.WeaponTypeID, attacker => attacker.FinalBlow,
                attacker => attacker.DamageDone, attacker => attacker.CharacterID, attacker => attacker.CorporationID,
                attacker => attacker.AllianceID, attacker => attacker.FactionID)
        {
        }
    }

    public class VictimDataReader : NestedKillDataReader<Victim>
    {
        public VictimDataReader(IEnumerable<Kill> parentValues)
            : base(
                parentValues, kill => new List<Victim> {kill.Victim}.GetEnumerator(), victim => victim.ShipTypeID,
                victim => victim.DamageTaken, victim => victim.CharacterID, victim => victim.CorporationID,
                victim => victim.AllianceID, victim => victim.FactionID)
        {
        }
    }

    public abstract class NestedKillDataReader<T> : NestedDataReader<Kill, T>
    {
        private readonly int _fieldCount;

        protected NestedKillDataReader(IEnumerable<Kill> parentValues, Func<Kill, IEnumerator<T>> getChildEnumerator,
                                       params Expression<Func<T, object>>[] mappings)
            : base(parentValues, getChildEnumerator, mappings)
        {
            _nameMappings["KillID"] = _mappings.Length;
            _fieldCount = _mappings.Length + 1;
        }

        public override int FieldCount
        {
            get { return _fieldCount; }
        }

        public override object GetValue(int i)
        {
            return i == _mappings.Length
                       ? _parentEnumerator.Current.KillID
                       : _mappings[i].Invoke(_childEnumerator.Current);
        }
    }

    public class DictReader : BaseDataReader
    {
        private readonly string _idName;
        private Dictionary<int, string>.Enumerator _moep;
        private ISet<int> writtenValues =new HashSet<int>();

        public DictReader(Dictionary<int, string> moep, string idName)
        {
            _idName = idName;
            _moep = moep.GetEnumerator();
        }

        public override object GetValue(int i)
        {
            if (i == 0)
            {
                if (writtenValues.Contains(_moep.Current.Key))
                {
                    
                    Console.WriteLine("strange: {0};{1}" ,_moep.Current.Key, _moep.Current.Value);
                    Read();
                    return GetValue(0);
                }
                writtenValues.Add(_moep.Current.Key);
                return _moep.Current.Key;
            }
            return _moep.Current.Value;
            //return i == 0 ? (object)_moep.Current.Key : _moep.Current.Value;
        }

        public override int GetOrdinal(string name)
        {
            return name == _idName ? 0 : 1;
        }

        public override int FieldCount
        {
            get {return 2; }
        }

        public override void Close()
        {
            Dispose();
        }

        public override bool Read()
        {
            return _moep.MoveNext();
        }

        public override void Dispose()
        {
            _moep.Dispose();
        }
    }

    internal class Program
    {
        private static string _curFile;
        private static int BATCH_SIZE = 1000;

        private static void Main(string[] args)
        {
            var start = DateTime.Now;
            ImportNames();
           
           // ImportKills();

            Console.WriteLine("done in {0}s", (DateTime.Now - start).TotalMinutes);
            Console.ReadKey();
        }

        private static void ImportNames()
        {
            var start = DateTime.Now;
            var directories = Directory.GetDirectories(".");
            Dictionary<int, string> characters = new Dictionary<int, string>();
            Dictionary<int, string> corporations = new Dictionary<int, string>();
            Dictionary<int, string> alliances = new Dictionary<int, string>();
            var allFiles =
                directories.SelectMany(Directory.GetFiles)
                           .SelectMany(
                               x =>
                               JsonConvert.DeserializeObject<IDictionary<long, string>>(File.ReadAllText(x)).Values);

            {
                Parallel.ForEach(allFiles, x =>
                    {
                        var curKill = GetKill(x);
                        if (curKill == null || curKill.KillID < 0)
                        {
                            return;
                        }

                        foreach (var curChar in curKill.Attackers)
                        {
                            characters[curChar.CharacterID] = curChar.CharacterName;
                            corporations[curChar.CorporationID] = curChar.CorporationName;
                            alliances[curChar.AllianceID] = curChar.AllianceName;
                        }
                        var victim = curKill.Victim;
                        characters[victim.CharacterID] = victim.CharacterName;
                        corporations[victim.CorporationID] = victim.CorporationName;
                        alliances[victim.AllianceID] = victim.AllianceName;
                    });
            }
            Console.WriteLine("read all files in {0}s", (DateTime.Now - start).TotalSeconds);
            using (
                var connection =
                    new SqlConnection(
                        "Server=rischwa.net;Database=kb_analysis;MultipleActiveResultSets=true;Persist Security Info=True;User ID=killboard;Password=!K1llb0ard!")
                )
            {
                connection.Open();
                WriteDings(connection, "CharacterDatas", "Character", characters);
                WriteDings(connection, "CorporationDatas", "Corporation", corporations);
                WriteDings(connection, "AllianceDatas", "Alliance", alliances);
            }

            Console.WriteLine("overall {0}s", (DateTime.Now - start).TotalSeconds);
        }

        private static void WriteDings(SqlConnection connection, string destinationTableName, string destinationColumnBase, 
                                       Dictionary<int, string> values)
        {
            var dbStart = DateTime.Now;

            using (
                var bulkCopy = new SqlBulkCopy(connection)
                    {
                        BatchSize = BATCH_SIZE,
                        BulkCopyTimeout = 180,
                        DestinationTableName = destinationTableName
                    })
            {
                bulkCopy.ColumnMappings.Add(destinationColumnBase + "ID", destinationColumnBase + "ID");
                bulkCopy.ColumnMappings.Add(destinationColumnBase + "Name", destinationColumnBase + "Name");
                bulkCopy.WriteToServer(new DictReader(values, destinationColumnBase + "ID"));
                bulkCopy.Close();
            }
            Console.WriteLine("wrote {0} in {1}s", destinationTableName, (DateTime.Now - dbStart).TotalSeconds);
        }

        private static void ImportKills()
        {
            var lastFilename = File.Exists("lastFile") ? File.ReadAllText("lastFile") : null;
            var hasFoundLastFile = lastFilename == null ? true : false;

            var directories = Directory.GetDirectories(".");
            foreach (var curDir in directories)
            {
                var files = Directory.GetFiles(curDir);
                foreach (var curFile in files)
                {
                    if (!hasFoundLastFile)
                    {
                        if (curFile == lastFilename)
                        {
                            hasFoundLastFile = true;
                        }
                        continue;
                    }

                    ImportFile(curFile);
                    File.WriteAllText("lastFile", curFile);
                }
            }
        }

        private const SqlBulkCopyOptions OPTIONS =
            SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.UseInternalTransaction;
        private static void ImportFile(string curFile)
        {
            _curFile = curFile;
            var start = DateTime.Now;
            var basic = JsonConvert.DeserializeObject<IDictionary<long, string>>(File.ReadAllText(curFile)).Values;
            var killsInFile = basic.AsParallel().Select(GetKill).Where(x=>x!= null && x.KillID > 0).ToList();
          //  Console.WriteLine("Read file '{0}' in {1}s", curFile, (DateTime.Now - start).TotalSeconds);

            using (var connection = new SqlConnection("Server=rischwa.net;Database=kb_analysis;MultipleActiveResultSets=true;Persist Security Info=True;User ID=killboard;Password=!K1llb0ard!"))
            {
                connection.Open();

                var dbStart = DateTime.Now;
            var t0=   Task.Run(() =>
                    {    using (
                    var bulkCopy = new SqlBulkCopy(connection, OPTIONS, null)
                        {
                            BatchSize = BATCH_SIZE,
                            BulkCopyTimeout = 180,
                            DestinationTableName = "Kills"
                        })
                {
                    bulkCopy.WriteToServer(new KillDataReader(killsInFile));
                    bulkCopy.Close();
                }   }
                    );
           //     Console.WriteLine("Wrote kills in {0}s", (DateTime.Now - dbStart).TotalSeconds);

              //  dbStart = DateTime.Now;
             var t1=   Task.Run(() =>
                    {
                        using (
                            var bulkCopy = new SqlBulkCopy(connection, OPTIONS, null)
                                {
                                    BatchSize = BATCH_SIZE,
                                    BulkCopyTimeout = 180,
                                    DestinationTableName = "Victims"
                                })
                        {
                            var reader = new VictimDataReader(killsInFile);
                            reader.AddMappings(bulkCopy);
                            bulkCopy.WriteToServer(reader);
                            bulkCopy.Close();
                        }
                    }
                    );
         //       Console.WriteLine("Wrote victims in {0}s", (DateTime.Now - dbStart).TotalSeconds);

              //  dbStart = DateTime.Now;
               var t2=   Task.Run(() =>
                    { using (
                    var bulkCopy = new SqlBulkCopy(connection, OPTIONS, null)
                        {
                            BatchSize = BATCH_SIZE,
                            BulkCopyTimeout = 180,
                            DestinationTableName = "Attackers"
                        })
                {
                    var reader = new AttackerDataReader(killsInFile);
                    reader.AddMappings(bulkCopy);
                    bulkCopy.WriteToServer(reader);
                    bulkCopy.Close();
                }   }
                    );
            //    Console.WriteLine("Wrote attackers in {0}s", (DateTime.Now - dbStart).TotalSeconds);

             //   dbStart = DateTime.Now;
              var t3=   Task.Run(() =>
                    {  using (
                    var bulkCopy = new SqlBulkCopy(connection, OPTIONS, null)
                        {
                            BatchSize = BATCH_SIZE,
                            BulkCopyTimeout = 180,
                            DestinationTableName = "Items"
                        })
                {
                    var reader = new ItemDataReader(killsInFile);
                    reader.AddMappings(bulkCopy);
                    bulkCopy.WriteToServer(reader);
                    bulkCopy.Close();
                }
                    }
                    );

                t0.Wait();
                t1.Wait();
                t2.Wait();
                t3.Wait();
                //  Console.WriteLine("Wrote items in {0}s", (DateTime.Now - dbStart).TotalSeconds);
            }

           // Console.WriteLine("Overalltime for {1} kills: {0}s", (DateTime.Now - start).TotalSeconds, killsInFile.Count);
        }

        private static Kill GetKill(string arg)
        {
            try
            {
                return JsonConvert.DeserializeObject<Kill>(arg);
            }catch(Exception e)
            {
                File.AppendAllText("errors.log",
                                   string.Format("error in '{0}':\n{1}:\n{2}", _curFile, e.Message, e.StackTrace));
                return null;
            }
        }
    }
}