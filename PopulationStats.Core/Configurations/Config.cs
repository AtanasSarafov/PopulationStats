namespace PopulationStats.Core.Configurations
{
    // Provides utility methods.
    public static class Config
    {
        // Dictionary for mapping country names to standardized names.
        private static readonly Dictionary<string, string> CountryMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // TODO:  If this list of country mappings will grow significantly,
            // or if it needs to be updated dynamically, consider loading this data from
            // an external source or configuration.
            { "U.S.A", "United States of America" },
            { "United States", "United States of America" },
            { "UK", "United Kingdom" },
            { "England", "United Kingdom" },
            // Add more mappings as needed
        };

        /// <summary>
        /// Gets the standardized country name for a given country name.
        /// </summary>
        /// <param name="countryName">The country name to standardize.</param>
        /// <returns>The standardized country name if a mapping exists; otherwise, returns the original name.</returns>
        public static string GetStandardizedCountryName(string countryName)
        {
            if (CountryMapping.TryGetValue(countryName, out var standardizedName))
            {
                return standardizedName;
            }
            // TODO: Optionally log or handle cases where the country name is not found.
            return countryName;
        }
    }
}
