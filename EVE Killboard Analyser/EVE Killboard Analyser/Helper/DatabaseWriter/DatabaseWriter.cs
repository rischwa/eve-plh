using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Entity;
using System.Threading;
using EVE_Killboard_Analyser.Models;
using log4net;

namespace EVE_Killboard_Analyser.Helper.DatabaseWriter
{
    public abstract class DatabaseWriter<T>  where T : class
    {
        protected static readonly BlockingCollection<T> WriteQueue = new BlockingCollection<T>();
        private static readonly ILog LOG = LogManager.GetLogger(typeof(DatabaseWriter<>));
        private Thread _thread;

        protected void Start()
        {
            lock (this)
            {
                if (_thread != null)
                {
                    return;
                }
                _thread = new Thread(() =>
                    {
                        while (true)
                        {
                            try
                            {
                                WriteNextEntryToDatabase();
                            }
                            catch (Exception e)
                            {
                            }
                        }
                    }) {IsBackground = true};
                _thread.Start();
            }
        }

        public void Add(T entry)
        {
            if (WriteQueue.Count < MaxEntryCount)
            {
                WriteQueue.Add(entry);
            }
        }

        public void ForceAdd(T entry)
        {
            WriteQueue.Add(entry);
        }

        protected abstract int MaxEntryCount { get; }

        protected abstract void WriteNextEntryToDatabase();

        protected void InsertOrUpdate(int characterID, T data, Func<DatabaseContext, DbSet<T>> getSet) 
        {
            using (var context = new DatabaseContext())
            {
                UpsertData(characterID, data, getSet, context);

              //  UpsertLastRequest(characterID, context);

                context.SaveChanges();
            }
        }

        private static void UpsertData(int characterID, T data, Func<DatabaseContext, DbSet<T>> getSet, DatabaseContext context) 
        {
            var set = getSet(context);
            var entry = set.Find(characterID);
            if (entry == null)
            {
                set.Add(data);
            }
            else
            {
                set.Remove(entry);
                //context.SaveChanges();
                set.Add(data);
              //  context.Entry(entry).State = EntityState.Detached;
               // set.Attach(data);
                //context.Entry(data).State = EntityState.Modified;
            }
        }

        //private static void UpsertLastRequest(int characterID, DatabaseContext context) 
        //{
        //    var request = context.Requests.Find(characterID);
        //    var updatedRequest = new KillboardRequest
        //        {
        //            CharacterID = characterID,
        //            LastAccess = DateTime.UtcNow
        //        };
        //    if (request == null)
        //    {
        //        context.Requests.Add(updatedRequest);
        //    }
        //    else
        //    {
        //        context.Entry(request).CurrentValues.SetValues(updatedRequest);
        //    }
        //}
    }
}