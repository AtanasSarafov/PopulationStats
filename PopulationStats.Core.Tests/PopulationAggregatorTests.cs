using Microsoft.EntityFrameworkCore;
using Moq;
using PopulationStats.Core.Interfaces;
using PopulationStats.Core.Models;
using PopulationStats.Core.Services;
using PopulationStats.Data.Models;

namespace PopulationStats.Core.Tests.Services
{
    public class PopulationAggregatorTests
    {
        private PopulationStatsDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<PopulationStatsDbContext>()
                          .UseInMemoryDatabase(databaseName: dbName)
                          .Options;

            var context = new PopulationStatsDbContext(options);
            context.Database.EnsureCreated();

            return context;
        }

        private List<Country> GetSeedCountries()
        {
            return new List<Country>
            {
                new Country
                {
                    CountryId = 1,
                    CountryName = "CountryA",
                    States = new List<State>
                    {
                        new State
                        {
                            StateId = 1,
                            StateName = "StateA",
                            Cities = new List<City>
                            {
                                new City { CityId = 1, CityName = "CityA", Population = 1000 },
                                new City { CityId = 2, CityName = "CityB", Population = 2000 }
                            }
                        },
                        new State
                        {
                            StateId = 2,
                            StateName = "StateB",
                            Cities = new List<City>
                            {
                                new City { CityId = 3, CityName = "CityC", Population = 1500 }
                            }
                        }
                    }
                },
                new Country
                {
                    CountryId = 2,
                    CountryName = "CountryB",
                    States = new List<State>
                    {
                        new State
                        {
                            StateId = 3,
                            StateName = "StateC",
                            Cities = new List<City>
                            {
                                new City { CityId = 4, CityName = "CityD", Population = 2500 }
                            }
                        }
                    }
                }
            };
        }

        [Fact]
        public async Task GetTotalPopulationByCountryAsync_ReturnsCorrectAggregatedResults()
        {
            var context = GetInMemoryDbContext("PopulationTestDB");
            context.Countries.AddRange(GetSeedCountries());
            context.SaveChanges();

            var mockStatService = new Mock<IStatService>();
            mockStatService.Setup(service => service.GetCountryPopulationsAsync())
                           .ReturnsAsync(new List<CountryPopulation>
                           {
                               new CountryPopulation("CountryA", 5000),
                               new CountryPopulation("CountryC", 3000)
                           });

            var aggregator = new PopulationAggregator(context, new List<IStatService> { mockStatService.Object });

            var result = await aggregator.GetTotalPopulationByCountryAsync();

            Assert.Equal(1000 + 2000 + 1500, result["CountryA"]); // 1000+2000+1500 from DB (+5000 from the stat service if we handle the overlaps)
            Assert.Equal(2500, result["CountryB"]);
            Assert.Equal(3000, result["CountryC"]); // Only from stat service
        }

        [Fact]
        public async Task GetPopulationDetailsAsync_ReturnsCorrectHierarchy()
        {
            var context = GetInMemoryDbContext("PopulationTestDB_Details");
            context.Countries.AddRange(GetSeedCountries());
            context.SaveChanges();

            var mockStatService = new Mock<IStatService>();
            var aggregator = new PopulationAggregator(context, new List<IStatService> { mockStatService.Object });
            var result = await aggregator.GetPopulationDetailsAsync();

            Assert.True(result.ContainsKey("CountryA"));
            Assert.True(result["CountryA"].ContainsKey("StateA"));
            Assert.True(result["CountryA"]["StateA"].ContainsKey("CityA"));
            Assert.Equal(1000, result["CountryA"]["StateA"]["CityA"]);

            Assert.True(result.ContainsKey("CountryB"));
            Assert.True(result["CountryB"].ContainsKey("StateC"));
            Assert.True(result["CountryB"]["StateC"].ContainsKey("CityD"));
            Assert.Equal(2500, result["CountryB"]["StateC"]["CityD"]);
        }
    }
}
