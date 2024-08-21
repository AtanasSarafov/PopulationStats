using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PopulationStats.Core.Models;
using PopulationStats.Core.Interfaces;
using System.Text.Json;

namespace PopulationStats.Core.Services
{
    /// <summary>
    /// Service to fetch and cache real-world population statistics from an external API.
    /// </summary>
    public class RealWorldStatService : IStatService
    {
        private readonly HttpClient httpClient;
        private readonly IMemoryCache cache;
        private readonly ILogger<RealWorldStatService> logger;
        private readonly TimeSpan cacheTtl;
        private readonly string apiUrl;

        private const int TtlDefault = 5;
        private const string CountriesApiUrlDefault = "https://restcountries.com/v3.1/all";

        /// <summary>
        /// Initializes a new instance of the <see cref="RealWorldStatService"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client to use for API calls.</param>
        /// <param name="cache">The memory cache for storing fetched data.</param>
        /// <param name="configuration">The configuration object to retrieve settings.</param>
        /// <param name="logger">The logger instance for logging errors and information.</param>
        public RealWorldStatService(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration, ILogger<RealWorldStatService> logger)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.apiUrl = configuration.GetValue("CountriesApiUrl", CountriesApiUrlDefault);

            var ttlMinutes = configuration.GetValue("CountryPopulationCacheTTLMinutes", TtlDefault);
            cacheTtl = TimeSpan.FromMinutes(ttlMinutes);
        }

        /// <summary>
        /// Asynchronously retrieves the population data for countries.
        /// </summary>
        /// <returns>A list of <see cref="CountryPopulation"/> representing the populations of countries.</returns>
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
                    logger.LogError("Failed to fetch data from the countries API. Status code: {StatusCode}", response.StatusCode);
                    return new List<CountryPopulation>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var countriesData = JsonDocument.Parse(content).RootElement;

                var countryPopulations = new List<CountryPopulation>();

                foreach (var country in countriesData.EnumerateArray())
                {
                    // Extract the common country name
                    var countryName = country.GetProperty("name").GetProperty("common").GetString();
                    if (string.IsNullOrEmpty(countryName))
                    {
                        logger.LogWarning("Country name missing in the data.");
                        continue;
                    }

                    // Extract population
                    if (country.TryGetProperty("population", out var populationProperty))
                    {
                        if (populationProperty.TryGetInt32(out int population))
                        {
                            countryPopulations.Add(new CountryPopulation(countryName, population));
                        }
                        else
                        {
                            logger.LogWarning("Invalid population value for country: {CountryName}", countryName);
                        }
                    }
                    else
                    {
                        logger.LogWarning("Population data missing for country: {CountryName}", countryName);
                    }
                }

                // Cache the data
                cache.Set("CountryPopulations", countryPopulations, cacheTtl);

                return countryPopulations;
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "JSON error while parsing country populations.");
                return new List<CountryPopulation>();
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

        /// <summary>
        /// Synchronously retrieves the population data for countries. This method is not recommended for use in production.
        /// </summary>
        /// <returns>A list of <see cref="CountryPopulation"/> representing the populations of countries.</returns>
        public List<CountryPopulation> GetCountryPopulations()
        {
            logger.LogWarning("Synchronous method called. It's recommended to use the asynchronous version for better performance.");
            return GetCountryPopulationsAsync().GetAwaiter().GetResult();
        }
    }
}
