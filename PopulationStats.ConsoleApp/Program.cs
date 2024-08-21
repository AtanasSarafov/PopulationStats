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

            using (var scope = serviceProvider.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var aggregator = services.GetRequiredService<IPopulationAggregator>();

                    Console.WriteLine($"--- Population counts per country ---");
                    await PrintTotalPopulationByCountry(aggregator);

                    Console.WriteLine($"--- Population counts per country [FROM CACHE] ---");
                    await PrintTotalPopulationByCountry(aggregator);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while running the population aggregation.");
                }
            }
        }

        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Add configuration, logging, and other necessary services
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();


            services.AddSingleton<IConfiguration>(configuration);

            services.AddLogging(configure => configure.AddConsole());

            services.AddDbContext<PopulationStatsDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

            services.AddMemoryCache();

            services.AddTransient<IStatService, ConcreteStatService>();

            services.AddSingleton<IStatService, RealWorldStatService>(provider =>
            {
                var cache = provider.GetRequiredService<IMemoryCache>();
                var logger = provider.GetRequiredService<ILogger<RealWorldStatService>>();
                return new RealWorldStatService(cache, configuration, logger);
            });

            services.AddTransient<IPopulationAggregator, PopulationAggregator>(provider =>
            {
                var context = provider.GetRequiredService<PopulationStatsDbContext>();
                var statServices = provider.GetServices<IStatService>().ToList();
                return new PopulationAggregator(context, statServices);
            });

            return services.BuildServiceProvider();
        }

        // Prints the population counts per country.
        private static async Task PrintTotalPopulationByCountry(IPopulationAggregator aggregator)
        {
            var stopwatch = Stopwatch.StartNew();
            var totalPopulations = await aggregator.GetTotalPopulationByCountryAsync();
            stopwatch.Stop();

            foreach (var kvp in totalPopulations)
            {
                Console.WriteLine($"{kvp.Key}: {kvp.Value}");
            }
            Console.WriteLine($"\nData aggregation completed in {stopwatch.ElapsedMilliseconds} ms.\n");
        }

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
