using Microsoft.EntityFrameworkCore;
using PopulationStats.Core.Configurations;
using PopulationStats.Core.Interfaces;
using PopulationStats.Data.Models;

namespace PopulationStats.Core.Services
{
    public class PopulationAggregator : IPopulationAggregator
    {
        private readonly PopulationStatsDbContext db;
        private readonly List<IStatService> statServices;

        public PopulationAggregator(PopulationStatsDbContext context, List<IStatService> statServices)
        {
            db = context;
            this.statServices = statServices; 
        }

        public async Task<Dictionary<string, int>> GetTotalPopulationByCountryAsync()
        {
            var populationByCountry = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            var dbPopulation = await (from c in db.Countries
                                      from s in c.States
                                      from city in s.Cities
                                      group city by c.CountryName into g
                                      select new
                                      {
                                          CountryName = Config.GetStandardizedCountryName(g.Key),
                                          Population = g.Sum(city => city.Population)
                                      })
                                      .ToListAsync();

            foreach (var entry in dbPopulation)
            {
                populationByCountry[entry.CountryName] = entry.Population.GetValueOrDefault();
            }

            // Aggregate population from other services.
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
                    }
                }
            }

            return populationByCountry;
        }

        public async Task<Dictionary<string, Dictionary<string, Dictionary<string, int>>>> GetPopulationDetailsAsync()
        {
            // Retrieve data from the database.
            var populationData = await (
                from country in db.Countries
                join state in db.States on country.CountryId equals state.CountryId
                join city in db.Cities on state.StateId equals city.StateId
                select new
                {
                    CountryName = country.CountryName,
                    StateName = state.StateName,
                    CityName = city != null ? city.CityName : "",
                    CityPopulation = city != null ? city.Population : 0,
                }).ToListAsync();

            // Aggregate data by country, state, and city.
            var result = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();

            foreach (var entry in populationData)
            {
                if (!result.ContainsKey(entry.CountryName))
                {
                    result[entry.CountryName] = new Dictionary<string, Dictionary<string, int>>();
                }

                if (!result[entry.CountryName].ContainsKey(entry.StateName))
                {
                    result[entry.CountryName][entry.StateName] = new Dictionary<string, int>();
                }

                result[entry.CountryName][entry.StateName][entry.CityName] = entry.CityPopulation.GetValueOrDefault();
            }

            return result;
        }
    }
}