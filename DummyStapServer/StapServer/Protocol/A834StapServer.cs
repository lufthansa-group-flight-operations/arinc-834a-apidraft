//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using Microsoft.Extensions.Logging;
using StapServer.Hardware;

namespace StapServer.Protocol
{
    public class A834StapServer
    {
        private readonly ILogger logger;
        private readonly StapClientConnectionHandler connectionHandler;
        private readonly IHardware hardware;

        public A834StapServer(ILogger<A834StapServer> logger, IHardware hardware, StapClientConnectionHandler handler)
        {
            this.hardware = hardware;
            this.logger = logger;
            this.logger.LogDebug("Create Stap Server");
            connectionHandler = handler;
        }

        public void Start()
        {
            logger.LogDebug("Start Stap Server");
            hardware.Init();
            hardware.Start();
            connectionHandler.Start();
        }

        public void Stop()
        {
            logger.LogDebug("Stop Stap Server");
            connectionHandler.Stop();
            hardware.Stop();
            logger.LogDebug("Stap Server Stopped");
        }
    }
}