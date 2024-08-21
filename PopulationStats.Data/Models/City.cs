using System.ComponentModel.DataAnnotations;

namespace PopulationStats.Data.Models
{
    public class City
    {
        [Key]
        public int CityId { get; set; }

        public required string CityName { get; set; }

        public int StateId { get; set; }

        public int? Population { get; set; }

        public required State State { get; set; }
    }
}
