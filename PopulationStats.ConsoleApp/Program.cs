using PopulationStats.Core.Data;
using PopulationStats.Core.Interfaces;
using System.Data.Common;

namespace PopulationStats.ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Started");
            Console.WriteLine("Getting DB Connection...");

            IDbManager db = new SqliteDbManager();
            DbConnection conn = db.GetConnection();

            if (conn == null)
            {
                Console.WriteLine("Failed to get connection");
            }
        }
    }
}
