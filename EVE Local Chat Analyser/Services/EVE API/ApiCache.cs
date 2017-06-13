using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EveLocalChatAnalyser.Utilities;

namespace EveLocalChatAnalyser.Services.EVE_API
{
    public static class ApiCache
    {
        private static readonly IDictionary<string, ICachedUntil> IN_MEMORY_CACHE = new Dictionary<string, ICachedUntil>();

        public static async Task<T> GetCurrentData<T, TStore>(string cachingId, Func<CachedData<TStore>> getFreshData,  Func<TStore, T> mapFromStorage, bool useDiskStore = true) where TStore : class where T : class
        {
            return await TaskEx.Run(() => GetCurrentData2(cachingId, getFreshData, mapFromStorage, useDiskStore));
        }

        private static T GetCurrentData2<T, TStore>(string cachingId, Func<CachedData<TStore>> getFreshData,  Func<TStore, T> mapFromStorage, bool useDiskStore )  where TStore :class where T:class
        {
            //TODO ueberpruiefung eigentlich falsch
           
            ICachedUntil localData;
            lock (IN_MEMORY_CACHE)
            {
                if (IN_MEMORY_CACHE.TryGetValue(cachingId, out localData) && localData.CachedUntil > DateTime.UtcNow)
                {
                    return ((CachedData<T>) localData).Value;
                }
            }
            var data = (CachedData<T>) localData;

            if (!useDiskStore)
            {
                var save = getFreshData();
                data = new CachedData<T>
                {
                    CachedUntil = save.CachedUntil,
                    Value = mapFromStorage(save.Value),
                    Id = cachingId
                };
                lock (IN_MEMORY_CACHE)
                {
                    IN_MEMORY_CACHE[cachingId] = data;
                }
                return data.Value;
            }

            using (var session = App.CreateStorageEngine())
            {
                var collection = session.GetCollection<CachedData<TStore>>(cachingId);

                if (data == null)
                {
                    CachedData<TStore>
                       storeData = collection.All()
                        .FirstOrDefault();
                    if (storeData != null)
                    {
                        data = new CachedData<T>
                               {
                                   CachedUntil = storeData.CachedUntil,
                                   Value = mapFromStorage(storeData.Value),
                                   Id = cachingId
                               };
                    }
                }
                if (data == null || data.CachedUntil < DateTime.UtcNow)
                {
                    var save = getFreshData();
                    save.Id = cachingId;

                    data = new CachedData<T>
                    {
                        CachedUntil = save.CachedUntil,
                        Value = mapFromStorage(save.Value),
                        Id = cachingId
                    };

                    collection.Upsert(save);
                }

                lock (IN_MEMORY_CACHE)
                {
                    IN_MEMORY_CACHE[cachingId] = data;
                }
            }
            return data.Value;
        }
    }
}
