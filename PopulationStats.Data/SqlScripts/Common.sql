-- ## Queries for testing purposes ##

-- Population by Country.
SELECT Country.CountryName, State.StateName, SUM(City.Population) AS TotalPopulation
FROM Country
JOIN State ON Country.CountryId = State.CountryId
JOIN City ON State.StateId = City.StateId
GROUP BY Country.CountryName

-- Country (Population total count)
-- 	State(Population count)
--		City(Population count)
SELECT 
    c.CountryName,
    s.StateName,
    ci.CityName,
    ci.Population AS CityPopulation,
    SUM(ci.Population) OVER (PARTITION BY s.StateName) AS StatePopulation,
    SUM(ci.Population) OVER (PARTITION BY c.CountryName) AS CountryPopulation
FROM 
    Country c
INNER JOIN 
    State s ON c.CountryId = s.CountryId
INNER JOIN 
    City ci ON s.StateId = ci.StateId
ORDER BY 
    c.CountryName, s.StateName, ci.CityName;
