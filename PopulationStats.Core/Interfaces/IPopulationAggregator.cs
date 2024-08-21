namespace PopulationStats.Core.Interfaces
{
    /// <summary>
    /// Defines methods for aggregating population data.
    /// </summary>
    public interface IPopulationAggregator
    {
        /// <summary>
        /// Asynchronously retrieves the total population for each country.
        /// </summary>
        /// <returns>A dictionary where the key is the country name and the value is the total population of that country.</returns>
        Task<Dictionary<string, int?>> GetTotalPopulationByCountryAsync();

        /// <summary>
        /// Asynchronously retrieves detailed population data, organized by country, state, and city.
        /// </summary>
        /// <returns>A dictionary where the key is the country name, the value is a dictionary of states, each of which contains a dictionary of cities and their populations.</returns>
        Task<Dictionary<string, Dictionary<string, Dictionary<string, int?>>>> GetPopulationDetailsAsync();
    }
}
