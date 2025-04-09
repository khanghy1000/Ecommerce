using System.Net.Http.Headers;
using System.Text.Json;
using Ecommerce.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Test;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing AppDbContext registration
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>)
            );
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Re-register DbContextOptions<AppDbContext> for PostgreSQL
            services.AddSingleton<DbContextOptions<AppDbContext>>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
                return optionsBuilder.Options;
            });

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Resolve the configuration from the service provider
            var configuration = sp.GetRequiredService<IConfiguration>();

            // Use the existing PostgreSQL database
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            });

            // Create a scope to obtain a reference to the database context
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<AppDbContext>();
                var logger = scopedServices.GetRequiredService<
                    ILogger<CustomWebApplicationFactory>
                >();

                // Ensure the database is created
                db.Database.EnsureCreated();

                try
                {
                    // Seed the database with test data if necessary
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "An error occurred seeding the database. Error: {Message}",
                        ex.Message
                    );
                }
            }
        });
    }

    public async Task<string> GetAuthTokenAsync(HttpClient client, string username, string password)
    {
        var loginData = new { email = username, password = password };

        var response = await client.PostAsJsonAsync("/api/login", loginData);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(content);
        var accessToken = document.RootElement.GetProperty("accessToken").GetString();

        return accessToken!;
    }
}
