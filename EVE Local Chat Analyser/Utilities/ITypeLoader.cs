using System.Threading.Tasks;

namespace EveLocalChatAnalyser.Utilities
{
    public interface ITypeLoader
    {
        Task<TypeInfo[]> LoadShipTypes();
    }
}