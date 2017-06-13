using EveLocalChatAnalyser.Ui.Models;
using GraphX.Controls;
using QuickGraph;

namespace EveLocalChatAnalyser.Ui.Map
{
    public class MapArea :
        GraphArea<SolarSystemViewModel, SolarSystemConnection, BidirectionalGraph<SolarSystemViewModel, SolarSystemConnection>>
    {
    }
}
