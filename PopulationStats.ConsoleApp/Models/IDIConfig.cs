using Microsoft.Extensions.DependencyInjection;

namespace PopulationStats.ConsoleApp.Models
{
    public interface IDIConfig
    {
        ServiceProvider ConfigureServices();
    }
}
