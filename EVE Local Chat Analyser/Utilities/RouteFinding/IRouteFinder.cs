namespace EveLocalChatAnalyser.Utilities.RouteFinding
{
    public interface IRouteFinder
    {
        StaticSolarSystemInfo[] GetRouteBetween(string source, string target);

        IRouteFinderOptions Options { get; set; }
    }
}