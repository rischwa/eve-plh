using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace EveLocalChatAnalyser.Utilities
{
    public class SQLiteTypeStorage : ITypeStorage
    {
        private readonly string _connectionString;

        public SQLiteTypeStorage(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<TypeInfo> ShipTypes
        {
            get
            {
                var result = new List<TypeInfo>();
                using (var dbConnection = new SQLiteConnection(_connectionString))
                {
                    dbConnection.Open();

                    var command = dbConnection.CreateCommand();
                    command.CommandText = "SELECT TypeID, TypeName FROM Type";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.NextResult())
                        {
                            var id = reader.GetInt32(0);
                            var name = reader.GetString(1);

                            result.Add(new TypeInfo(id, name));
                        }
                    }
                }
                return result;
            }
        }

        public void SetTypes(IEnumerable<TypeInfo> types)
        {
            using (var dbConnection = new SQLiteConnection(_connectionString))
            {
                dbConnection.Open();

                using (var transaction = dbConnection.BeginTransaction(IsolationLevel.Serializable))
                {

                    var command = dbConnection.CreateCommand();
                    command.CommandText = "DELETE FROM Type";
                    command.ExecuteNonQuery();

                    var insertCommand = dbConnection.CreateCommand();
                    insertCommand.CommandText = "INSERT INTO Type VALUES(@typeId, @typeName)";
                    var typeIdParameter = new SQLiteParameter("typeId", DbType.Int32);
                    var nameParameter = new SQLiteParameter("typeName", DbType.String);
                    insertCommand.Parameters.Add(typeIdParameter);
                    insertCommand.Parameters.Add(nameParameter);

                    foreach (var curInfo in types)
                    {
                        typeIdParameter.Value = curInfo.TypeID;
                        nameParameter.Value = curInfo.Name;
                        insertCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }
    }
}
