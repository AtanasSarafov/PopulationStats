using PopulationStats.Core.Models;

namespace PopulationStats.Core.Interfaces
{
    public interface IStatService
    {
        List<CountryPopulation> GetCountryPopulations();
        Task<List<CountryPopulation>> GetCountryPopulationsAsync();
    }
}
