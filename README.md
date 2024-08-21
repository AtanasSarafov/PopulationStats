# PopulationStats
A .NET solution for simple tasks (population statistics aggregation), designed as a technical discussion starter during interviews.


## Features

**PopulationStats** is a .NET application that aggregates and displays population data for countries, states, and cities. It integrates data from various sources and provides basic reports on population statistics.

- **Total Population by Country**: Aggregates and displays the total population for each country.
- **Detailed Population Data**: Provides hierarchical details of population by country, state, and city. (optional)
- **Data Integration**: Fetches data from external sources and a local SQLite database.
- **Caching**: Implements in-memory caching for performance optimization.
- **Error Handling and Logging**: Basic error handling and logging mechanisms for monitoring and debugging.

## Project Structure

- **PopulationStats.ConsoleApp**: Console application for executing the population statistics aggregation and reporting.
  - `Program.cs`: Entry point for the console application.
- **PopulationStats.ConsoleApp.Tests**: Unit tests for the console application.
  - `ProgramTests.cs`: Unit tests for `Program.cs`.
- **PopulationStats.Core**: Core application logic and services.
  - **Configurations**: Configuration-related utilities.
    - `Config.cs`: Helper methods for country name standardization.
  - **Interfaces**: Interfaces for core services.
    - `IPopulationAggregator.cs`: Interface for population aggregation.
    - `IStatService.cs`: Interface for statistical services.
  - **Models**: Data models used in the application.
    - `CountryPopulation.cs`: Represents country population data.
  - **Services**: Implementations of core services.
    - `ConcreteStatService.cs`: Provides static population data.
    - `PopulationAggregator.cs`: Aggregates population data from various sources.
    - `RealWorldStatService.cs`: Fetches real-world population data from external APIs.
- **PopulationStats.Core.Tests**: Unit tests for core services.
  - `PopulationAggregatorTests.cs`: Unit tests for `PopulationAggregator.cs`.
- **PopulationStats.Data**: Data models and database context.
  - **Database**: SQLite database files.
    - `citystatecountry.db`: SQLite database file.
  - **Models**: Data models for database entities.
    - `City.cs`: Represents city data.
    - `Country.cs`: Represents country data.
    - `State.cs`: Represents state data.
    - `PopulationStatsDbContext.cs`: Entity Framework DbContext for database operations.

## Getting Started

### Prerequisites

- .NET 6 or later
- SQLite
- Visual Studio or another compatible IDE

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/populationstats.git

2. Navigate to the project directory:
   ```bash
   cd populationstats

3. Restore the NuGet packages:
   ```bash
   dotnet restore

4. Build the solution:
   ```bash
   dotnet build

5. Run the console application:
   ```bash
   dotnet run --project PopulationStats.ConsoleApp
      
### Configuration

- **appsettings.json**: Configure application settings, including API URLs and cache TTL. Example configuration:
  ```json
  {
    "CountriesApiUrl": "https://restcountries.com/v3.1/all",
    "CountryPopulationCacheTTLMinutes": 5,
    "ConnectionStrings": {
      "DefaultConnection": "Data Source=path_to_your_database.db"
    }
  }


##  Overall Improvements Suggestions

- **Caching**:
If the application needs to scale across multiple instances or handle large amounts of data, it's recommended to integrate Redis with IDistributedCache in ASP.NET Core. This will significantly improve scalability and performance. However, for simpler scenarios within a single instance, the memory cache is sufficient.

- **Exception Handling**:
Implement global exception handling using middleware. This helps in capturing unhandled exceptions and provides a consistent way to return error responses.

- **Logging**: 
Use structured logging with a tool like Serilog or NLog. 
Structured logging allows better querying and analysis of log data. 
Ensure that logs are comprehensive and include enough context for debugging.

- **Testing**:
Implement integration tests to verify the interactions between components and external systems.

## Suggested New Features

- **User Authentication and Authorization**:
Use ASP.NET Core Identity or OAuth providers for this purpose.

- **Advanced Analytics and Reporting**:
Add features for advanced analytics, such as generating custom reports, visualizations, or dashboards. Use libraries like D3.js or Chart.js for visualizations.

- **Data Export and Import**:
Implement functionality to export data to formats like CSV or Excel and import data from these formats.

- **Multi-language Support**:
Add internationalization support to make the application usable in multiple languages. Use ASP.NET Core’s localization features to manage translations and culture-specific content.

- **API:**
expose the provided features though API enpoints (OpenAPI generator could be use to generate code based on specification). Enhance the API with rate limiting and throttling.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contact

For any questions or feedback, please contact atanas.sarafov@gmail.com.

