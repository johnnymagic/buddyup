using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using BuddyUp.API.Data;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;

namespace BuddyUp.API.Configuration
{
    public static class PostgreSqlConfiguration
    {
        public static IServiceCollection AddPostgreSqlDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    // Configure Npgsql-specific options
                    npgsqlOptions.UseNetTopologySuite(); // For spatial data
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                });
                
                // Set naming convention to lowercase
                // options.UseSnakeCaseNamingConvention(); // Removed as it is now handled in ApplicationDbContext
                
                // Add logging in development
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });
            
            return services;
        }
        
        public static IApplicationBuilder EnsureDatabaseCreated(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                // Check if connection can be established
                try
                {
                    dbContext.Database.CanConnect();
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
                    logger.LogError(ex, "An error occurred connecting to the database.");
                    throw;
                }
            }
            
            return app;
        }
    }
}