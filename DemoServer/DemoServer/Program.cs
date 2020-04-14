//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using System;
using DemoServer.DataAccess;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DemoServer
{
    /// <summary>
    /// Main program entry point.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    var context = services.GetRequiredService<DatabaseContext>();
                    // context.Database.Migrate();
                    context.Database.EnsureCreated();
                    InitializeDatabase(services, logger);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to initialize the Database.");
                }
            }

            host.Run();
        }

        /// <summary>
        /// Gets a web host builder.
        /// </summary>
        /// <param name="args">Arguments to use.</param>
        /// <returns>Created web host builder.</returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        /// <summary>
        /// Initializes the database.
        /// </summary>
        /// <param name="service">Service provider to use.</param>
        /// <param name="logger">Logger to use.</param>
        private static void InitializeDatabase(IServiceProvider service, ILogger logger)
        {
            using var context = new DatabaseContext(service.GetRequiredService<DbContextOptions<DatabaseContext>>());

            // Look for any aircraft.
            if (context.Messages.AnyAsync().Result)
            {
                logger.LogDebug("Database is already initialized.");
                return;
            }

            logger.LogDebug("Initialize Database ...");
        }
    }
}