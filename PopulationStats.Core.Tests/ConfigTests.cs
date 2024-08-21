using PopulationStats.Core.Configurations;

namespace PopulationStats.Core.Tests
{
    public class ConfigTests
    {
        [Theory]
        [InlineData("U.S.A", "United States of America")]
        [InlineData("United States", "United States of America")]
        [InlineData("UK", "United Kingdom")]
        [InlineData("England", "United Kingdom")]
        public void GetStandardizedCountryName_ShouldReturnStandardizedName(string input, string expected)
        {
            var result = Config.GetStandardizedCountryName(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("Brazil", "Brazil")]
        [InlineData("Argentina", "Argentina")]
        public void GetStandardizedCountryName_ShouldReturnOriginalName_WhenNotMapped(string input, string expected)
        {
            var result = Config.GetStandardizedCountryName(input);
            Assert.Equal(expected, result);
        }
    }
}
