using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace MyForum.Api.Infrastructure.Data;

public class ForumDbContextFactory : IDesignTimeDbContextFactory<ForumDbContext>
{
    public ForumDbContext CreateDbContext(string[] args)
    {
        var environment =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("ForumDbContext");

        var csb = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Password = configuration["POSTGRES_PASSWORD"] ??
                        throw new InvalidOperationException("POSTGRES_PASSWORD is not set"),
            Username = configuration["POSTGRES_USER"] ??
                        throw new InvalidOperationException("POSTGRES_USER is not set")
        };

        if (string.IsNullOrWhiteSpace(csb.ConnectionString))
            throw new InvalidOperationException(
                "Connection string 'ForumDbContext' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<ForumDbContext>();
        optionsBuilder.UseNpgsql(csb.ConnectionString);

        return new ForumDbContext(optionsBuilder.Options);
    }
}
