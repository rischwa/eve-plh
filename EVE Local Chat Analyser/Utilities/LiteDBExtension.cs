using LiteDB;

namespace EveLocalChatAnalyser.Utilities
{
    public static class LiteDBExtension
    {
        public static void Upsert<T>(this Collection<T> collection, T value) where T : new()
        {
            if (!collection.Update(value))
            {
                collection.Insert(value);
            }
        }

        public static Collection<T> GetCollection<T>(this LiteEngine engine) where T :new()
        {
            return engine.GetCollection<T>(typeof (T).Name);
        } 
    }
}
