using System;

namespace EveLocalChatAnalyser.Services.EVE_API
{
    public interface ICachedUntil
    {
        DateTime CachedUntil { get; set; }
    }

    public class CachedData<T> : ICachedUntil
    {
        public DateTime CachedUntil { get; set; }
        public T Value { get; set; } 

        public string Id { get; set; }
    }
}