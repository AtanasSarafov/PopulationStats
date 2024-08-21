using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PopulationStats.Core.Interfaces;
using PopulationStats.Core.Services;
using PopulationStats.Data.Models;
using System.Diagnostics;

namespace PopulationStats.ConsoleApp
{
    internal class Program
    {
        static async Task Main()
        {
            var serviceProvider = ConfigureServices();

            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                var aggregator = services.GetRequiredService<IPopulationAggregator>();

                await DisplayPopulationDataAsync(aggregator, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while running the population aggregation.");
            }
        }

        private static ServiceProvider ConfigureServices()
        {
            var configuration = BuildConfiguration();

            var services = new ServiceCollection();
            services.AddSingleton(configuration);

            // Logging setup
            services.AddLogging(configure => configure.AddConsole());

            // Database setup
            services.AddDbContext<PopulationStatsDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

            // Caching and services registration
            services.AddMemoryCache();
            services.AddTransient<IStatService, ConcreteStatService>();
            services.AddSingleton<IStatService, RealWorldStatService>(provider =>
            {
                var cache = provider.GetRequiredService<IMemoryCache>();
                var logger = provider.GetRequiredService<ILogger<RealWorldStatService>>();
                return new RealWorldStatService(cache, configuration, logger);
            });

            // Population Aggregator setup
            services.AddTransient<IPopulationAggregator, PopulationAggregator>(provider =>
            {
                var context = provider.GetRequiredService<PopulationStatsDbContext>();
                var statServices = provider.GetServices<IStatService>().ToList();
                return new PopulationAggregator(context, statServices);
            });

            return services.BuildServiceProvider();
        }

        private static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        private static async Task DisplayPopulationDataAsync(IPopulationAggregator aggregator, ILogger logger)
        {
            logger.LogInformation("Starting population aggregation.");

            await MeasureAndDisplayPopulationDataAsync(
                aggregator.GetTotalPopulationByCountryAsync,
                "Population counts per country",
                logger
            );

            await MeasureAndDisplayPopulationDataAsync(
               aggregator.GetTotalPopulationByCountryAsync,
               "[WITH CASHE] Population counts per country",
               logger
           );

            logger.LogInformation("Population aggregation completed.");
        }

        private static async Task MeasureAndDisplayPopulationDataAsync(
            Func<Task<Dictionary<string, int>>> fetchPopulationData,
            string header,
            ILogger logger)
        {
            logger.LogInformation($"--- {header} ---");

            var stopwatch = Stopwatch.StartNew();
            var populationData = await fetchPopulationData();
            stopwatch.Stop();

            DisplayPopulationByCountry(populationData);
            logger.LogInformation($"Data aggregation completed in {stopwatch.ElapsedMilliseconds} ms.\n");
        }

        private static void DisplayPopulationByCountry(Dictionary<string, int> populationData)
        {
            foreach (var (country, population) in populationData)
            {
                Console.WriteLine($"{country}: {population}");
            }
        }

        // TODO: For testing purposes. Remove when not needed.
        // Prints the population counts in  hierarchical format.
        private static async Task PrintPopulationDetails(IPopulationAggregator aggregator)
        {
            // Country(Population total count)
            //    State(Population count)
            //       City(Population count)
            var stopwatch = Stopwatch.StartNew();
            var populationDetails = await aggregator.GetPopulationDetailsAsync();
            stopwatch.Stop();
            Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds}ms");

            foreach (var countryEntry in populationDetails)
            {
                Console.WriteLine($"{countryEntry.Key} ({countryEntry.Value.Values.SelectMany(state => state.Values).Sum()}):");
                foreach (var stateEntry in countryEntry.Value)
                {
                    Console.WriteLine($"\t{stateEntry.Key} ({stateEntry.Value.Values.Sum()}):");
                    foreach (var cityEntry in stateEntry.Value)
                    {
                        Console.WriteLine($"\t\t{cityEntry.Key} ({cityEntry.Value})");
                    }
                }
            }
        }
    }
}
