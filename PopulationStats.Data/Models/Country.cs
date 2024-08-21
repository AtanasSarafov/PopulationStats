using System.ComponentModel.DataAnnotations;

namespace PopulationStats.Data.Models
{
    public class Country
    {
        [Key]
        public int CountryId { get; set; }

        public string? CountryName { get; set; }

        public ICollection<State>? States { get; set; }
    }
}
