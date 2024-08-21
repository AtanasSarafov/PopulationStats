using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PopulationStats.Core.Models;
using PopulationStats.Core.Services;
using System.Net;

namespace PopulationStats.Core.Tests
{
    public class RealWorldStatServiceTests
    {
        private readonly IConfiguration configuration;
        private readonly IMemoryCache cache;
        private readonly Mock<ILogger<RealWorldStatService>> logger;
        private HttpClient httpClient;

        private const string Ttl = "5";
        private const string ApiUrl = "https://restcountries.com/v3.1/all";

        public RealWorldStatServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string> {
                { "CountriesApiUrl", ApiUrl },
                { "CountryPopulationCacheTTLMinutes", Ttl }
            };
            configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            cache = new MemoryCache(new MemoryCacheOptions());
            logger = new Mock<ILogger<RealWorldStatService>>();
        }

        [Fact]
        public async Task GetCountryPopulationsAsync_ShouldReturnPopulations_WhenApiCallIsSuccessful()
        {
            var apiResponse = "[{\"name\": {\"common\": \"Country1\"}, \"population\": 1000}, {\"name\": {\"common\": \"Country2\"}, \"population\": 2000}]";
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, apiResponse);
            httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiUrl) };
            var service = new RealWorldStatService(httpClient, cache, configuration, logger.Object);

            var result = await service.GetCountryPopulationsAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, cp => cp.CountryName == "Country1" && cp.Population == 1000);
            Assert.Contains(result, cp => cp.CountryName == "Country2" && cp.Population == 2000);
        }

        [Fact]
        public async Task GetCountryPopulationsAsync_ShouldReturnCachedPopulations_WhenCacheIsAvailable()
        {
            var cachedPopulations = new List<CountryPopulation>
            {
                new CountryPopulation("CachedCountry1", 5000),
                new CountryPopulation("CachedCountry2", 6000)
            };
            httpClient = new HttpClient(new MockHttpMessageHandler()) { BaseAddress = new Uri(ApiUrl) };
            cache.Set("CountryPopulations", cachedPopulations);
            var service = new RealWorldStatService(httpClient, cache, configuration, logger.Object);

            var result = await service.GetCountryPopulationsAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, cp => cp.CountryName == "CachedCountry1" && cp.Population == 5000);
            Assert.Contains(result, cp => cp.CountryName == "CachedCountry2" && cp.Population == 6000);
        }

        [Fact]
        public async Task GetCountryPopulationsAsync_ShouldLogError_WhenApiCallFails()
        {
            var handler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError, string.Empty);
            httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiUrl) };
            var service = new RealWorldStatService(httpClient, cache, configuration, logger.Object);

            await service.GetCountryPopulationsAsync();

            logger.Verify(logger => logger.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                    It.Is<EventId>(eventId => eventId.Id == 0),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to fetch data from the countries API.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetCountryPopulationsAsync_ShouldLogError_WhenParsingJsonFails()
        {
            var apiResponse = "Invalid JSON";
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, apiResponse);
            httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiUrl) };
            var service = new RealWorldStatService(httpClient, cache, configuration, logger.Object);

            await service.GetCountryPopulationsAsync();

            logger.Verify(logger => logger.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                    It.Is<EventId>(eventId => eventId.Id == 0),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("JSON error while parsing country populations.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Helper class for mocking HttpMessageHandler
    internal class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseContent;

        public MockHttpMessageHandler(HttpStatusCode statusCode = HttpStatusCode.OK, string responseContent = "")
        {
            _statusCode = statusCode;
            _responseContent = responseContent;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseContent)
            };

            return Task.FromResult(response);
        }
    }
}
