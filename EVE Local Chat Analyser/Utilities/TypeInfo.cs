namespace EveLocalChatAnalyser.Utilities
{
    public class TypeInfo
    {
        public TypeInfo(int typeID, string name)
        {
            TypeID = typeID;
            Name = name;
        }

        public string Name { get; }

        public int TypeID { get; }
    }
}