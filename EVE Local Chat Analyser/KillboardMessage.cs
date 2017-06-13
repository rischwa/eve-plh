namespace EveLocalChatAnalyser
{
    internal class KillboardMessage
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public KillboardInformation Data { get; set; }
        public string Stacktrace { get; set; }
    }
}