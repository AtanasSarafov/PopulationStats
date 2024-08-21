using PopulationStats.Core.Interfaces;
using PopulationStats.Core.Models;

namespace PopulationStats.Core.Services
{
    /// <summary>
    /// A concrete implementation of the IStatService interface that returns hardcoded country population data.
    /// </summary>
    public class ConcreteStatService : IStatService
    {
        /// <summary>
        /// Retrieves a list of country populations synchronously.
        /// </summary>
        /// <returns>A list of <see cref="CountryPopulation"/> objects with hardcoded data.</returns>
        public List<CountryPopulation> GetCountryPopulations()
        {
            return new List<CountryPopulation>
            {
                new CountryPopulation("India", 1182105000),        //DB: India
                new CountryPopulation("United Kingdom", 62026962), //DB: United Kingdom
                new CountryPopulation("Chile", 17094270),          //DB: Chile
                new CountryPopulation("Mali", 15370000),           //DB: Mali
                new CountryPopulation("Greece", 11305118),
                new CountryPopulation("Armenia", 3249482),
                new CountryPopulation("Slovenia", 2046976),
                new CountryPopulation("Saint Vincent and the Grenadines", 109284),
                new CountryPopulation("Bhutan", 695822),
                new CountryPopulation("Aruba (Netherlands)", 101484),
                new CountryPopulation("Maldives", 319738),
                new CountryPopulation("Mayotte (France)", 202000),
                new CountryPopulation("Vietnam", 86932500),
                new CountryPopulation("Germany", 81802257),
                new CountryPopulation("Botswana", 2029307),
                new CountryPopulation("Togo", 6191155),
                new CountryPopulation("Luxembourg", 502066),
                new CountryPopulation("U.S. Virgin Islands (US)", 106267),
                new CountryPopulation("Belarus", 9480178),
                new CountryPopulation("Myanmar", 59780000),
                new CountryPopulation("Mauritania", 3217383),
                new CountryPopulation("Malaysia", 28334135),
                new CountryPopulation("Dominican Republic", 9884371),
                new CountryPopulation("New Caledonia (France)", 248000),
                new CountryPopulation("Slovakia", 5424925),
                new CountryPopulation("Kyrgyzstan", 5418300),
                new CountryPopulation("Lithuania", 3329039),
                new CountryPopulation("United States of America", 309349689) //DB: U.S.A.
            };
        }

        /// <summary>
        /// Retrieves a list of country populations asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with a list of <see cref="CountryPopulation"/> objects as the result.</returns>
        public Task<List<CountryPopulation>> GetCountryPopulationsAsync()
        {
            return Task.FromResult(GetCountryPopulations());
        }
    }
}
