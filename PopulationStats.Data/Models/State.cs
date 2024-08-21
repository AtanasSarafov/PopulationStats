using System.ComponentModel.DataAnnotations;

namespace PopulationStats.Data.Models
{
    public class State
    {
        [Key]
        public int StateId { get; set; }

        public required string StateName { get; set; }

        public int CountryId { get; set; }

        public virtual Country Country { get; set; } = null!;

        public ICollection<City>? Cities { get; set; }
    }
}
