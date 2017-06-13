using EveLocalChatAnalyser.Utilities;

namespace EveLocalChatAnalyser.Ui
{
    public static class ShipTypes
    {
        private static IEveTypes _delegate;
        public static IEveTypes Instance => _delegate ?? (_delegate = DIContainer.GetInstance<IEveTypes>());
    }
}