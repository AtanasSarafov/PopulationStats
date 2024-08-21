using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PopulationStats.ConsoleApp.Models;
using PopulationStats.Core.Interfaces;
using System.Diagnostics;

namespace PopulationStats.ConsoleApp
{
    public class Program
    {
        private readonly IDIConfig _diConfig;

        public Program(IDIConfig diConfig)
        {
            _diConfig = diConfig;
        }

        static async Task Main()
        {
            var program = new Program(new DIConfig());
            await program.Run();
        }

        public async Task Run()
        {
            var serviceProvider = _diConfig.ConfigureServices();

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

        public static async Task PrintTotalPopulationByCountry(IPopulationAggregator aggregator)
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

        // TODO: For testing purposes. Remove when not needed.
        // Prints the population counts in  hierarchical format:
        // Country(Population total count)
        //    State(Population count)
        //       City(Population count)
        public static async Task PrintPopulationDetails(IPopulationAggregator aggregator)
        {
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