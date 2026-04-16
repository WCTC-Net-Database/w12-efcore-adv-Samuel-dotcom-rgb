using ConsoleRpg.Helpers;
using ConsoleRpg.Services;
using ConsoleRpgEntities.Data;
using ConsoleRpgEntities.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;

namespace ConsoleRpg;

public static class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Build configuration
        var configuration = ConfigurationHelper.GetConfiguration();

        // Create and bind FileLoggerOptions
        var fileLoggerOptions = new NReco.Logging.File.FileLoggerOptions();
        configuration.GetSection("Logging:File").Bind(fileLoggerOptions);

        // Configure logging
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));

            // Add Console logger
            loggingBuilder.AddConsole();

            // Add File logger using the correct constructor
            var logFileName = "Logs/log.txt"; // Specify the log file path

            loggingBuilder.AddProvider(new FileLoggerProvider(logFileName, fileLoggerOptions));
        });

        // Register DbContext with dependency injection
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<GameContext>(options =>
        {
            ConfigurationHelper.ConfigureDbContextOptions(options, connectionString!);
        });


        // Register your services
        services.AddTransient<GameEngine>();
        services.AddTransient<MenuManager>();
        services.AddSingleton<OutputManager>();
    }
}
