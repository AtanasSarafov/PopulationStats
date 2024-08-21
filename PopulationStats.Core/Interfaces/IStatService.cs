using PopulationStats.Core.Models;

namespace PopulationStats.Core.Interfaces
{
    /// <summary>
    /// Defines methods for retrieving statistic data.
    /// </summary>
    public interface IStatService
    {
        /// <summary>
        /// Retrieves a list of country populations synchronously.
        /// </summary>
        /// <returns>A list of <see cref="CountryPopulation"/> objects representing country names and their populations.</returns>
        List<CountryPopulation> GetCountryPopulations();

        /// <summary>
        /// Retrieves a list of country populations asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with a list of <see cref="CountryPopulation"/> objects as the result.</returns>
        Task<List<CountryPopulation>> GetCountryPopulationsAsync();
    }
}
