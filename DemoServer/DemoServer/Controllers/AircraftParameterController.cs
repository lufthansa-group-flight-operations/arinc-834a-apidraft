//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using System;
using System.Linq;
using System.Threading.Tasks;
using DemoServer.DataAccess;
using DemoServer.Models;
using DemoServer.Websocket;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DemoServer.Controllers
{
    [Route("api/v1/parameters")]
    [ApiController]
    public class AircraftParameterController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAvionicDataSource _avionicData;

        public AircraftParameterController(
            ILogger<AircraftParameterController> logger,
            IServiceProvider serviceProvider,
            IAvionicDataSource avionicData)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _avionicData = avionicData;
        }

        [HttpOptions]
        [Produces("application/json", "application/xml")]
        public IActionResult GetInfo()
        {
            return Ok(new AvionicParameterInfoList { Parameters = _avionicData.GetParameterInfos() });
        }

        [HttpGet]
        [Produces("application/json", "application/xml", "text/avionic")]
        public async Task<IActionResult> GetParameter()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                _logger.LogDebug($"Websocket request for {HttpContext.Request.Path}");
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                var client = _serviceProvider.GetService<IWebSocketClientHandler>();
                await client.StartListen(webSocket, HttpContext);
                client.Dispose();
                return new EmptyResult();
            }

            var req = Request.Query;
            var parameters = _avionicData.GetParameters();
            if (req.ContainsKey("keys"))
            {
                var filter = req["keys"].First().Split(",");
                parameters = parameters.Where(p => filter.Contains(p.Key)).ToArray();
            }

            var result = new AvionicParameters { Parameters = parameters };
            return Ok(result);
        }
    }
}