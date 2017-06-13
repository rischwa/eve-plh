using System;
using EveLocalChatAnalyser.Model;
using EveLocalChatAnalyser.Properties;
using EveLocalChatAnalyser.Services;
using EveLocalChatAnalyser.Ui.Map.Statistics;
using EveLocalChatAnalyser.Utilities.GateCampDetection;
using EveLocalChatAnalyser.Utilities.PositionTracking;
using EveLocalChatAnalyser.Utilities.PosMapper;
using EveLocalChatAnalyser.Utilities.QuickAction;
using EveLocalChatAnalyser.Utilities.RouteFinding;
using EveLocalChatAnalyser.Utilities.VoiceCommands;
using EveLocalChatAnalyser.Utilities.Win32;
using SimpleInjector;

namespace EveLocalChatAnalyser.Utilities
{
    internal static class DIContainer
    {
        public static readonly Container Container;

        static DIContainer()
        {
            Container = new Container();

            Container.Register(GetExternalCharacterService);

            Container.RegisterSingle<IShipClassifications, ShipClassifications>();
            
            Container.RegisterSingle<IActiveCharacterTracker, ActiveCharacterTracker>();
            Container.RegisterSingle<IPositionTracker, PositionTracker>();
            Container.RegisterSingle<LocalChatAnalyser>();
            Container.RegisterSingle<LogBasedPositionTracking>();
            Container.RegisterSingle<IHwndSource, HwndSource>();
            Container.RegisterSingle<IEveScoutService, EveScoutService>();
            
            Container.RegisterSingle<IClipboardHook, ClipboardHook>();
            Container.RegisterSingle<IKeyboardHook, KeyboardHook>();
            Container.RegisterSingle<ClipboardParser>();
            Container.RegisterSingle<IScanAccess, ScanAccess>();
            Container.RegisterSingle<IWormholeConnectionRepository, WormholeConnectionRepository>();
            Container.RegisterSingle<IWormholeConnectionTracker, WormholeConnectionTracker>();
            Container.RegisterSingle<IStaticUniverseData, StaticUniverseData>();
            Container.RegisterSingle<IRouteFinder, RouteFinder>();
            Container.Register<DScanFinder>();
            Container.Register<PosMapper2>();
            Container.RegisterSingle<ICoalitionService, CoalitionService>();
            Container.RegisterSingle<IQuickAction, QuickAction.QuickAction>();
            Container.RegisterSingle<IVoiceCommands, VoiceCommands.VoiceCommands>();
            Container.RegisterSingle<ICustomCharacterInfoRepository, CustomCharacterInfoRepository>();
            Container.RegisterSingle<ITypeStorage>(new SQLiteTypeStorage(@"data source=./Resources/plh_universe_data.sqlite3;Read Only=False"));
            Container.RegisterSingle<ITypeLoader, CrestTypeLoader>();
            Container.RegisterSingle<IEveTypes, EveTypes>();
            Container.RegisterSingle<IGateCampDetectionService, GateCampDetectionService>();
            Container.RegisterSingle<IMapGateCamps,MapGateCamps>();
        }

        private static IExternalKillboardService GetExternalCharacterService()
        {
            ExternalServiceType externalServiceType;
            var isValid = Enum.TryParse(Settings.Default.ExternalService, out externalServiceType);
            return isValid && externalServiceType == ExternalServiceType.EveKill ? (IExternalKillboardService)new EveKillService() : new ZKillboardService();
        }

        public static T GetInstance<T>() where T : class
        {
            return Container.GetInstance<T>();
        }
    }
}