//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using System;
using AutoMapper;
using DemoServer.DataAccess;
using DemoServer.Formatter;
using DemoServer.MappingProfiles;
using DemoServer.Websocket;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSwag;

namespace DemoServer
{
    /// <summary>
    /// Startup class for the web host.
    /// </summary>
    public class Startup : StartupBase
    {
        private readonly ILogger _logger;

        private IConfiguration Configuration { get; }
        private string _stapServerIpAddress;
        private int _stapServerIpPort;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="logger">Logger to use.</param>
        /// <param name="configuration">Service configuration.</param>
        public Startup(ILogger<Startup> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }

        /// <inheritdoc />
        public override void ConfigureServices(IServiceCollection services)
        {
            var databasefile = Configuration["Database"];
            _stapServerIpAddress = Configuration["StapServerIpAddress"];
            _stapServerIpPort = int.Parse(Configuration["StapServerIpPort"]);
            _logger.LogInformation($"Using Database:               [{databasefile}]");
            _logger.LogInformation($"Using Stap Server IP Address: [{_stapServerIpAddress}]");
            _logger.LogInformation($"Using Stap Server IP Port:    [{_stapServerIpAddress}]");

            services.AddDbContext<DatabaseContext>(opt => opt.UseSqlite($"Data Source={databasefile}"));
            services.AddResponseCompression();
            services.AddAutoMapper(typeof(DemoServerProfile));
            services.AddHealthChecks();
            services.AddControllers(
                    opt =>
                    {
                        opt.RespectBrowserAcceptHeader = true;
                        opt.OutputFormatters.Insert(0, new AvionicParameterOutputFormatter());
                    })
                .AddXmlSerializerFormatters()
                .AddJsonOptions(options => options.JsonSerializerOptions.IgnoreNullValues = true);

            services.AddSwaggerDocument(
                config =>
                {
                    config.PostProcess = document =>
                    {
                        document.Info.Version = "v1";
                        document.Info.Title = "ARINC 834A API";
                        document.Info.Description = "A simple ARINC 834A Demo Server";
                        document.Info.TermsOfService = "None";
                        document.Info.Contact = new OpenApiContact
                        {
                            Name = "John Doe",
                            Email = string.Empty,
                            Url = "https://github.com/jdoe"
                        };
                        document.Info.License = new OpenApiLicense
                        {
                            Name = "License Info",
                            Url = "https://example.com/license"
                        };
                    };
                });

            services.AddSingleton<IAvionicDataSource, AvionicDataSource>();
            services.AddTransient<IWebSocketClientHandler, WebSocketClientHandler>();
        }

        /// <inheritdoc />
        public override void Configure(IApplicationBuilder app)
        {
            var env = app.ApplicationServices.GetService<IWebHostEnvironment>();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var webSocketOptions = new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(5),
                ReceiveBufferSize = 1024,
            };

            // Added WebSocket functionality
            app.UseWebSockets(webSocketOptions);
            app.UseResponseCompression();
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(
                endpoints =>
                {
                    endpoints.MapHealthChecks("/health");
                    endpoints.MapControllers();
                });

            app.UseOpenApi();
            app.UseSwaggerUi3();
        }
    }
}