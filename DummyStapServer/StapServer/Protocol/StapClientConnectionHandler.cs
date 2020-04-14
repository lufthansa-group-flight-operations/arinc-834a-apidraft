//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StapServer.Hardware;

namespace StapServer.Protocol
{
    public class StapClientConnectionHandler
    {
        private readonly ILogger logger;
        private readonly IServiceProvider service;
        private readonly List<StapClientConnection> clientHandlerList;
        private readonly TcpListener tcplistener;
        private readonly IHardware hardware;

        private bool shouldrun;
        private int clientId;

        public StapClientConnectionHandler(ILogger<StapClientConnectionHandler> logger, IHardware hardware, IConfigurationRoot config, IServiceProvider service)
        {
            this.service = service;
            this.logger = logger;
            this.hardware = hardware;
            var port = int.Parse(config["StapServer:Port"]);
            clientHandlerList = new List<StapClientConnection>();
            tcplistener = new TcpListener(IPAddress.Any, port);
        }

        public void Start()
        {
            logger.LogDebug("Start");
            shouldrun = true;
            var waitForClientsThread = new Thread(WaitForClients) { IsBackground = true };
            waitForClientsThread.Start();
        }

        public void Stop()
        {
            logger.LogDebug("Stop");
            
            shouldrun = false;
            tcplistener.Stop();

            if (clientHandlerList.Count > 0)
            {
                foreach (var client in clientHandlerList)
                {
                    client.Stop();
                }

                clientHandlerList.Clear();
            }

            logger.LogDebug("Stopped");
        }

        private void WaitForClients()
        {
            tcplistener.Start();

            while (shouldrun)
            {
                logger.LogDebug("Wait for Clients");
                try
                {
                    TcpClient newClient = tcplistener.AcceptTcpClient();
                    logger.LogDebug("New Client connection");
                    var clientLogger = service.GetRequiredService<ILogger<StapClientConnection>>();
                    clientHandlerList.Add(new StapClientConnection(clientLogger, hardware, newClient, clientId++));

                }
                catch (Exception ex)
                {
                    logger.LogWarning($"Error Waiting for clients: {ex.Message}");
                }
            }
        }
    }
}