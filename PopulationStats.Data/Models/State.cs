using System.ComponentModel.DataAnnotations;

namespace PopulationStats.Data.Models
{
    public class State
    {
        [Key]
        public int StateId { get; set; }

        public required string StateName { get; set; }

        public int CountryId { get; set; }

        public required Country Country { get; set; }

        public ICollection<City>? Cities { get; set; }
    }
}
