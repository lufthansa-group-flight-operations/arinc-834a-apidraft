//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using StapServer.Hardware;
using StapServer.Protocol;

namespace StapServer
{
    class Program
    {
        static void Main(string[] args)
        {
            using var services = ConfigureServices();
            var config = services.GetRequiredService<IConfigurationRoot>();
            var logger = services.GetRequiredService<ILogger<Program>>();

            ushort port;
            if (!ushort.TryParse(config["StapServer:Port"], out port))
            {
                logger.LogError("Failed to parse port number.");
                return;
            }

            logger.LogInformation("### STAP SERVER ###");
            logger.LogInformation($"Starting on {port}");
            logger.LogInformation("Hit Enter to stop");
            var server = services.GetRequiredService<A834StapServer>();
            server.Start();
            Console.ReadLine();
            server.Stop();
            logger.LogInformation("Hit Enter to exit");
            Console.ReadLine();
        }

        private static ServiceProvider ConfigureServices()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json", true, true)
                .Build();

            var services = new ServiceCollection()
                .AddLogging(
                    logging =>
                    {
                        logging.AddConfiguration(config.GetSection("Logging"));
                        logging.AddConsole(
                            option =>
                            {
                                option.Format = ConsoleLoggerFormat.Systemd;
                                option.TimestampFormat = " HH:mm:ss.ffff ";
                            });
                    })
                .AddTransient<Program>()
                .AddSingleton(config)
                .AddSingleton<A834StapServer>()
                .AddSingleton<StapClientConnectionHandler>()
                .AddSingleton<IHardware, TestHardware>();

            return services.BuildServiceProvider();
        }
    }
}