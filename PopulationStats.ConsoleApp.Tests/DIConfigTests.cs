using Microsoft.Extensions.DependencyInjection;
using PopulationStats.Core.Interfaces;

namespace PopulationStats.ConsoleApp.Tests
{
    public class DIConfigTests
    {
        [Fact]
        public void ConfigureServices_ShouldReturnServiceProvider()
        {
            // Act
            var serviceProvider = new DIConfig().ConfigureServices();

            // Assert
            Assert.NotNull(serviceProvider);
            var aggregator = serviceProvider.GetService<IPopulationAggregator>();
            Assert.NotNull(aggregator);
        }
    }
}
