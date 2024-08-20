using Microsoft.Data.Sqlite;
using PopulationStats.Core.Interfaces;
using System.Data.Common;

namespace PopulationStats.Core.Data
{
    public class SqliteDbManager : IDbManager
    {
        public DbConnection GetConnection()
        {
            try
            {
                var connection = new SqliteConnection("Data Source=..\\..\\..\\..\\PopulationStats.Data\\Database\\citystatecountry.db;Mode=ReadWrite");
                connection.Open();
                return connection;
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
