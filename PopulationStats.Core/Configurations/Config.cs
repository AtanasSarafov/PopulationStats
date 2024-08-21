namespace PopulationStats.Core.Configurations
{
    public static class Config
    {
        private static readonly Dictionary<string, string> CountryMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "U.S.A", "United States of America" },
            { "United States", "United States of America" },
            { "UK", "United Kingdom" },
            { "England", "United Kingdom" },
            // Add more mappings as needed
        };

        public static string GetStandardizedCountryName(string countryName)
        {
            return CountryMapping.ContainsKey(countryName) ? CountryMapping[countryName] : countryName;
        }
    }
}

