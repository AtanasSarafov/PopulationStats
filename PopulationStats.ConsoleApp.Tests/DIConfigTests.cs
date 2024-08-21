using Microsoft.Extensions.DependencyInjection;
using PopulationStats.Core.Interfaces;

namespace PopulationStats.ConsoleApp.Tests
{
    public class DIConfigTests
    {
        [Fact]
        public void ConfigureServices_ShouldReturnServiceProvider()
        {
            var serviceProvider = new DIConfig().ConfigureServices();

            Assert.NotNull(serviceProvider);
            var aggregator = serviceProvider.GetService<IPopulationAggregator>();
            Assert.NotNull(aggregator);
        }
    }
}
