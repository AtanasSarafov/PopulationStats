using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PopulationStats.ConsoleApp.Models;
using PopulationStats.Core.Interfaces;
using PopulationStats.Core.Services;
using PopulationStats.Data.Models;

namespace PopulationStats.ConsoleApp
{
    public class DIConfig : IDIConfig
    {
        public ServiceProvider ConfigureServices()
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
    }
}
