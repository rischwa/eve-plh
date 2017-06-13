using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using EveLocalChatAnalyser.Properties;

namespace EveLocalChatAnalyser.Utilities.RouteFinding
{
    public class MySQLiteCommand<T>
    {
        private readonly SQLiteCommand _command;
        private readonly Func<SQLiteDataReader, T> _mapping;

        public MySQLiteCommand(SQLiteConnection connection, string query, Func<SQLiteDataReader, T> mapping)
        {
            _mapping = mapping;
            _command = connection.CreateCommand();
            _command.CommandText = query;
        }

        public IEnumerable<T> Execute()
        {
            using (var reader = _command.ExecuteReader())
            {
                return new ReaderEnumerable(reader, _mapping).ToList()
                                                             .AsReadOnly();
            }
        }

        private class ReaderEnumerable : IEnumerable<T>
        {
            private readonly ReaderEnumerator _enumerator;

            public ReaderEnumerable([NotNull] SQLiteDataReader reader, [NotNull] Func<SQLiteDataReader, T> mapping)
            {
                //TODO kann halt nur einmal benutzt werden ...
                _enumerator = new ReaderEnumerator(reader, mapping);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _enumerator;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class ReaderEnumerator : IEnumerator<T>
        {
            private readonly Func<SQLiteDataReader, T> _mapping;
            private readonly SQLiteDataReader _reader;

            public ReaderEnumerator([NotNull] SQLiteDataReader reader, [NotNull] Func<SQLiteDataReader, T> mapping)
            {
                _reader = reader;
                _mapping = mapping;
            }

            public T Current { get; private set; }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (!_reader.Read())
                {
                    Current = default(T);
                    return false;
                }

                Current = _mapping(_reader);
                return true;
            }

            public void Reset()
            {
                throw new NotImplementedException("Reset is not available for ReaderEnumerator");
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }
    }
}