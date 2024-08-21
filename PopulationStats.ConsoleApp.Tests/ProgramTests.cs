using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using PopulationStats.ConsoleApp.Models;
using PopulationStats.Core.Interfaces;

namespace PopulationStats.ConsoleApp.Tests
{
    public class ProgramTests
    {
        [Fact]
        public async Task Main_ShouldLogErrorOnException()
        {
            var mockDIConfig = new Mock<IDIConfig>();

            var serviceCollection = new ServiceCollection();
            var mockLogger = new Mock<ILogger<Program>>();

            serviceCollection.AddSingleton(mockLogger.Object);

            var faultyAggregator = new Mock<IPopulationAggregator>();
            faultyAggregator.Setup(a => a.GetTotalPopulationByCountryAsync())
                            .ThrowsAsync(new Exception("Test exception"));

            serviceCollection.AddSingleton(faultyAggregator.Object);

            mockDIConfig.Setup(config => config.ConfigureServices())
                .Returns(serviceCollection.BuildServiceProvider());

            var program = new Program(mockDIConfig.Object);

            await program.Run();

            mockLogger.Setup(x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Test exception")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()));
        }

        [Fact]
        public async Task PrintTotalPopulationByCountry_ShouldPrintCorrectOutput()
        {
            var mockAggregator = new Mock<IPopulationAggregator>();
            var expectedPopulations = new Dictionary<string, int?>
            {
                { "CountryA", 1000 },
                { "CountryB", 2000 }
            };

            mockAggregator.Setup(a => a.GetTotalPopulationByCountryAsync())
                          .ReturnsAsync(expectedPopulations);

            using var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            await Program.PrintTotalPopulationByCountry(mockAggregator.Object);

            var output = stringWriter.ToString();
            Assert.Contains("CountryA: 1000", output);
            Assert.Contains("CountryB: 2000", output);
            Assert.Contains("Data aggregation completed in", output);
        }

        [Fact]
        public async Task PrintPopulationDetails_ShouldPrintCorrectOutput()
        {
            var mockAggregator = new Mock<IPopulationAggregator>();
            var populationDetails = new Dictionary<string, Dictionary<string, Dictionary<string, int?>>>
            {
                {
                    "CountryA", new Dictionary<string, Dictionary<string, int?>>
                    {
                        {
                            "StateA", new Dictionary<string, int?>
                            {
                                { "CityA", 500 },
                                { "CityB", 300 }
                            }
                        },
                        {
                            "StateB", new Dictionary<string, int?>
                            {
                                { "CityC", 200 }
                            }
                        }
                    }
                }
            };

            mockAggregator.Setup(a => a.GetPopulationDetailsAsync())
                          .ReturnsAsync(populationDetails);

            using var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            await Program.PrintPopulationDetails(mockAggregator.Object);

            var output = stringWriter.ToString();
            Assert.Contains("CountryA (1000):", output);
            Assert.Contains("\tStateA (800):", output);
            Assert.Contains("\t\tCityA (500)", output);
            Assert.Contains("\t\tCityB (300)", output);
            Assert.Contains("\tStateB (200):", output);
            Assert.Contains("\t\tCityC (200)", output);
        }
    }
}
