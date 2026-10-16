using Microsoft.EntityFrameworkCore;
using RaceDay.Infrastructure.Data;

namespace RaceDay.Tests;

public static class TestHelpers
{
    public static RaceDayDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new RaceDayDbContext(options);
    }
}