using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PopulationStats.Core.Models;
using PopulationStats.Core.Interfaces;
using System.Text.Json;

namespace PopulationStats.Core.Services
{
    public class RealWorldStatService : IStatService
    {
        private readonly HttpClient httpClient;
        private readonly IMemoryCache cache;
        private readonly ILogger<RealWorldStatService> logger;
        private readonly TimeSpan cacheTtl;
        private readonly string apiUrl;

        private const int TtlDefailt = 5;
        private const string CountriesApiUrlDefault = "https://restcountries.com/v3.1/all";


        public RealWorldStatService(IMemoryCache cache, IConfiguration configuration, ILogger<RealWorldStatService> logger)
        {
            this.httpClient = new HttpClient();
            this.cache = cache;
            this.logger = logger;
            this.apiUrl = configuration.GetValue("CountriesApiUrl", CountriesApiUrlDefault);

            var ttlMinutes = configuration.GetValue("CountryPopulationCacheTTLMinutes", TtlDefailt);
            cacheTtl = TimeSpan.FromMinutes(ttlMinutes);
        }

        public List<CountryPopulation> GetCountryPopulations()
        {
            try
            {
                var result = GetCountryPopulationsAsync().GetAwaiter().GetResult();
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching country populations synchronously.");
                return new List<CountryPopulation>();
            }
        }

        public async Task<List<CountryPopulation>> GetCountryPopulationsAsync()
        {
            try
            {
                // Check if the data is already cached
                if (cache.TryGetValue("CountryPopulations", out List<CountryPopulation> cachedPopulation))
                {
                    return cachedPopulation;
                }

                var response = await httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("Failed to fetch data from the countries API.");
                }

                var content = await response.Content.ReadAsStringAsync();
                var countriesData = JsonDocument.Parse(content).RootElement;

                var countryPopulations = new List<CountryPopulation>();

                foreach (var country in countriesData.EnumerateArray())
                {

                    // Extract the common country name
                    var countryName = country.GetProperty("name").GetProperty("common").GetString();

                    // Extract population
                    if (country.TryGetProperty("population", out var populationProperty))
                    {
                        int population = populationProperty.GetInt32();
                        countryPopulations.Add(new CountryPopulation(countryName, population));
                    }
                }

                // Cache the data
                cache.Set("CountryPopulations", countryPopulations, cacheTtl);

                return countryPopulations;
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "HTTP error while fetching country populations.");
                return new List<CountryPopulation>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while fetching country populations.");
                return new List<CountryPopulation>();
            }
        }
    }
}
