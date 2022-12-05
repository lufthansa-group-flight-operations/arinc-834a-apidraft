//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DemoServer.DataAccess;
using DemoServer.Models;
using DemoServer.WebSockets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DemoServer.Controllers
{
    [Route("a834a/adif/v1/parameters")]
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

        [HttpGet]
        [Route("{name}")]
        public async Task<IActionResult> GetParameter(string name)
        {
            var result = _avionicData.GetParameter(name);
            if (result is null)
            {
                return NotFound("Parameter not found");
            }

            return Ok(result);
        }

        [HttpGet]        
        [Produces("application/json", "text/avionic")]
        public async Task<IActionResult> GetParameters([FromQuery(Name = "params")] List<string> paramsRequest)
        {
            _logger.LogDebug($"Requested parameter list from [{HttpContext.Connection.RemoteIpAddress}]");

            // If none paramters are requested, just return a list of the known paramter names, and if they are settable.
            if (paramsRequest.Count == 0)
            {
                return Ok(new AvionicParameters() { Parameters = _avionicData.KnownParams});
            }

            // By default supporting parameters requested as: /parameters?params={name}&params={name}&params={name}
            
            // Using a hashet instead of array to ensure requested parameters are only handled once.
            var uniqueRequest = new HashSet<string>();

            // 
            foreach (var item in paramsRequest)
            {
                // If an items contains a comma, it may be the array of parameters names to subscribe.
                if (item.Contains(","))
                {
                    // Handle parameters if they are delivered in the format of parameters?params={name},{name},{name}.
                    // Split the string into separate items, and also remove any spaces or empty entries.
                    var subItems = item.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                    // Merge with the hashset
                    uniqueRequest.UnionWith(subItems);
                }
                else
                {
                    // Handle parameter if requested as: /parameters?params={name}&params={name}&params={name}
                    uniqueRequest.Add(item.Trim());
                }
            }

            // Get requested paramters from the avionics-source-service
            var foundParameters = _avionicData.GetParameters().Where(p => uniqueRequest.Contains(p.Name)).ToArray();
            
            // Detect the parameters that were in the request but not in the foundParaeters to get the not found parameters.
            var notFoundParameters = uniqueRequest.Where(u => !foundParameters.Select(p => p.Name).Contains(u)).ToArray();

            // If none parameter found, return Not Found Error
            if (foundParameters.Length == 0)
            {
                return NotFound();
            }

            // Create response and fill it with the found parameters
            var result = new AvionicParameters { Parameters = foundParameters };

            // If some parameters were not found, attach their names
            if (notFoundParameters.Length > 0)
            {
                result.UnknownParameters = (string[]?)notFoundParameters;
            }

            //Return response
            return Ok(result);
        }

        /// WebSocket Subscription        
        [HttpGet]
        [Route("subscribe")]
        public async Task<IActionResult> HandleWebsocket()
        {
            // Check if the Request is a valid Websocket request
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                _logger.LogDebug($"Websocket request for {HttpContext.Request.Path} " +
                    $"with SubProtocols{HttpContext.WebSockets.WebSocketRequestedProtocols}");

                System.Net.WebSockets.WebSocket webSocket = null;

                if (HttpContext.WebSockets.WebSocketRequestedProtocols.Count > 0)
                {
                    if (HttpContext.WebSockets.WebSocketRequestedProtocols.Contains("adif-1"))
                    {
                        // Accept the WebSocket connection and return the client that 'adif-1' subprotocol is used.
                        webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync("adif-1");
                    }
                    else
                    {                        
                        return new BadRequestResult();
                    }
                }
                else
                {
                    webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                }
                

                // Retrieve a WebSocketHandler from Service-Repository
                var client = _serviceProvider.GetService<IWebSocketClientHandlerAcParameter>();

                // Handover the established websocketconnection to the clientHandler and Start it                
                await client.StartListen(webSocket, HttpContext);

                // If the clientHandler finishes (due to closed websocket connection)
                // int will end up here and we tidy up erverything
                client.Dispose();
                return new EmptyResult();
            }
            else
            {
                // If the request was not a valid WebSocket request, return BadRequest
                return BadRequest("Resource can only be accessed with WebSockets.");
            }
        }
    }
}