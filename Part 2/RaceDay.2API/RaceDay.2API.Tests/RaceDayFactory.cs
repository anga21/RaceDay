
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay._2API.Data;

namespace RaceDay._2API.Tests;

public class RaceDayFactory : WebApplicationFactory<Program>
{
    public const string OrganiserCode = "TEST-ORGANISER-CODE";

    // A unique database name per factory, so tests never share data
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("OrganiserRegistrationCode", OrganiserCode);

        builder.ConfigureServices(services =>
        {
            // Remove the SQL Server registration for the DbContext
            var toRemove = services
                .Where(d => d.ServiceType.IsGenericType &&
                            d.ServiceType.GetGenericArguments().Contains(typeof(RaceDayDbContext)))
                .ToList();
            foreach (var descriptor in toRemove)
                services.Remove(descriptor);

            // Use an in-memory database instead
            services.AddDbContext<RaceDayDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }
}