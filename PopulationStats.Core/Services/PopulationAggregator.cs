using Microsoft.EntityFrameworkCore;
using PopulationStats.Core.Configurations;
using PopulationStats.Core.Interfaces;
using PopulationStats.Data.Models;

namespace PopulationStats.Core.Services
{
    /// <summary>
    /// Aggregates population data from a database and various stat services.
    /// </summary>
    public class PopulationAggregator : IPopulationAggregator
    {
        private readonly PopulationStatsDbContext dbContext;
        private readonly List<IStatService> statServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="PopulationAggregator"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="statServices">The list of stat services.</param>
        public PopulationAggregator(PopulationStatsDbContext context, List<IStatService> statServices)
        {
            dbContext = context ?? throw new ArgumentNullException(nameof(context));
            this.statServices = statServices ?? throw new ArgumentNullException(nameof(statServices));
        }

        /// <summary>
        /// Retrieves the total population by country asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with a dictionary of country names and their populations as the result.</returns>
        public async Task<Dictionary<string, int?>> GetTotalPopulationByCountryAsync()
        {
            var populationByCountry = new Dictionary<string, int?>(StringComparer.OrdinalIgnoreCase);

            // Retrieve population data from the database.
            var dbPopulation = await (from c in dbContext.Countries
                                      from s in c.States
                                      from city in s.Cities
                                      group city by c.CountryName into g
                                      select new
                                      {
                                          CountryName = Config.GetStandardizedCountryName(g.Key),
                                          Population = g.Sum(city => city.Population)
                                      })
                                      .ToListAsync();

            // Add database population data to dictionary.
            foreach (var entry in dbPopulation)
            {
                populationByCountry[entry.CountryName] = entry.Population;
            }

            // Aggregate population data from stat services.
            foreach (var statService in statServices)
            {
                var serviceData = await statService.GetCountryPopulationsAsync();
                foreach (var countryPopulation in serviceData)
                {
                    var standardizedCountryName = Config.GetStandardizedCountryName(countryPopulation.CountryName);

                    if (!populationByCountry.ContainsKey(standardizedCountryName))
                    {
                        populationByCountry[standardizedCountryName] = countryPopulation.Population;
                    }
                    else
                    {
                        // TODO: Add all results that are not found in DB as a new Dictionary.
                        // Change the return type to be a model.
                        // Optional: Handle overlaps, e.g., sum or average populations.
                        // populationByCountry[standardizedCountryName] += countryPopulation.Population;
                    }
                }
            }

            return populationByCountry;
        }

        /// <summary>
        /// Retrieves detailed population information by country, state, and city asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with a hierarchical dictionary of population details as the result.</returns>
        public async Task<Dictionary<string, Dictionary<string, Dictionary<string, int?>>>> GetPopulationDetailsAsync()
        {
            // Retrieve population data from the database.
            var populationData = await (
                from country in dbContext.Countries
                join state in dbContext.States on country.CountryId equals state.CountryId
                join city in dbContext.Cities on state.StateId equals city.StateId
                select new
                {
                    CountryName = country.CountryName,
                    StateName = state.StateName,
                    CityName = city.CityName,
                    CityPopulation = city.Population,
                }).ToListAsync();

            // Aggregate data by country, state, and city.
            var result = new Dictionary<string, Dictionary<string, Dictionary<string, int?>>>();

            foreach (var entry in populationData)
            {
                if (!result.ContainsKey(entry.CountryName))
                {
                    result[entry.CountryName] = new Dictionary<string, Dictionary<string, int?>>();
                }

                if (!result[entry.CountryName].ContainsKey(entry.StateName))
                {
                    result[entry.CountryName][entry.StateName] = new Dictionary<string, int?>();
                }

                result[entry.CountryName][entry.StateName][entry.CityName] = entry.CityPopulation;
            }

            return result;
        }
    }
}
