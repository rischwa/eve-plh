namespace EveLocalChatAnalyser.Utilities
{
    public static class UnitConverter
    {
        public static double KmToAU(double km)
        {
            return km*6.68458712226844549599/1000000000.0;
        }

        public static double AUToKm(double au)
        {
            return au*149597870.7;
        }
    }
}
