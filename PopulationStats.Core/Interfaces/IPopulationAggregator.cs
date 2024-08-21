namespace PopulationStats.Core.Interfaces
{
    public interface IPopulationAggregator
    {
        Task<Dictionary<string, int>> GetTotalPopulationByCountryAsync();
        Task<Dictionary<string, Dictionary<string, Dictionary<string, int>>>> GetPopulationDetailsAsync();
    }
}
