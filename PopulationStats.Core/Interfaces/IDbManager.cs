using System.Data.Common;

namespace PopulationStats.Core.Interfaces
{

    public interface IDbManager
    {
        DbConnection GetConnection();
    }
}
