using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using EveLocalChatAnalyser.Ui.Map.Statistics.Palettes;
using EveLocalChatAnalyser.Ui.Settings;
using log4net;

namespace EveLocalChatAnalyser.Ui.Map.Statistics.Palettes
{
    public static class PaletteCollection
    {
        private static readonly ILog LOG = LogManager.GetLogger("PaletteCollection");
        public static PaletteViewModel[] Values => GetPalettes();

        public static PaletteViewModel GetByName(string name)
        {
            return Values.FirstOrDefault(x => x.Name == name) ?? new PaletteViewModel()
                                                                 {
                                                                     Name = "Fallback - red",
                                                                     Palette = new SingleHueRedPalette()
                                                                 };
        }

        private static  PaletteViewModel[] GetPalettes()
        {
            try
            {
                var iPaletteType = typeof(IPalette);
                return Assembly.GetAssembly(iPaletteType)
                    .GetTypes()
                    .Where(x => iPaletteType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Select(CreatePaletteViewModel)
                    .ToArray();
            }
            catch (Exception e)
            {
                LOG.Error("Could not load palettes", e);
                return new PaletteViewModel[0];
            }
        }
        
        private static PaletteViewModel CreatePaletteViewModel(Type arg)
        {
            return new PaletteViewModel { Name = arg.Name, Palette = (IPalette)Activator.CreateInstance(arg) };
        }
    }
}
