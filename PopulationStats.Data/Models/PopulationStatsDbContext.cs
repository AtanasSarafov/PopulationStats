using Microsoft.EntityFrameworkCore;

namespace PopulationStats.Data.Models
{
    public partial class PopulationStatsDbContext : DbContext
    {
        public PopulationStatsDbContext(DbContextOptions<PopulationStatsDbContext> options) : base(options)
        { }

        public virtual DbSet<City> Cities { get; set; }
        public virtual DbSet<State> States { get; set; }
        public virtual DbSet<Country> Countries { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Country>().ToTable("Country");
            builder.Entity<City>().ToTable("City");
            builder.Entity<State>().ToTable("State");
            base.OnModelCreating(builder);
        }
    }
}
