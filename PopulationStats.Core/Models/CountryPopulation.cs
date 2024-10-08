﻿namespace PopulationStats.Core.Models
{
    public class CountryPopulation
    {
        public string CountryName { get; set; }
        public int Population { get; set; }

        public CountryPopulation(string countryName, int population)
        {
            CountryName = countryName ?? throw new ArgumentNullException(nameof(countryName));
            Population = population;
        }
    }
}
