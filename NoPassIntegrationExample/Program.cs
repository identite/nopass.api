using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NoPassIntegrationExample.Data;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NoPassIntegrationExample
{
    public class Program
    {
        public static int Main(string[] args)
        {
            // Create the Logger Configuration
            var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Async(a => a.File(
                "NoPassIntegrationExampleLog.txt",
                fileSizeLimitBytes: null,
                rollingInterval: RollingInterval.Day))
            .WriteTo.Async(a => a.Console());

            // Create the Logger
            Log.Logger = loggerConfig.CreateLogger();

            var host = CreateHostBuilder(args)
                   .UseSerilog()
                   .Build();

            // Enable auto migrations
            try
            {
                using var scope = host.Services.CreateScope();
                var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
                context?.Database.Migrate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw;
            }

            host.Run();
            return 0;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
