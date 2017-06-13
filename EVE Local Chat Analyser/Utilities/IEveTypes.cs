namespace EveLocalChatAnalyser.Utilities
{
    public interface IEveTypes
    {
        string this[int typeId] { get; }

        bool IsShipTypeName(string name);

        bool IsPod(int shipTypeId);
    }
}