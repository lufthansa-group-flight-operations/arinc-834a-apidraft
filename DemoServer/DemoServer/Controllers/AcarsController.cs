using DemoServer.DataAccess;
using DemoServer.Models;
using DemoServer.Services.acars;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DemoServer.Controllers
{
    [Route("a834a/acars/v1/")]
    [ApiController]
    public class AcarsController : ControllerBase
    {
        private readonly IAcarsMessageService _acarsMessageService;
        private readonly IServiceProvider _serviceProvider;

        public AcarsController(IAcarsMessageService acarsMessageService, IServiceProvider serviceProvider)
        {
            this._acarsMessageService = acarsMessageService;
            this._serviceProvider = serviceProvider;
        }

        // GET: api/<AcarsController>
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var result = (_acarsMessageService.GetStatus());
            result.Selflink = GetSelflink();           
            return Ok(result);
        }

        // Get all uplinks
        [HttpGet("uplinks")]
        public async Task<IActionResult> GetUplinks(
            [FromHeader(Name = "If-None-Match")] string? eTag,
            [FromHeader(Name = "mti")] string? mti,
            [FromQuery(Name = "include_payload")] bool includeContent)
        {
            //TODO: Implement
            if (mti != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "MTI Filter not implemented");
            }
            //END TODO

            var result = new AcarsResponseMessageList()
            {
                SelfLink = GetSelflink(),
                Total = _acarsMessageService.Uplinks.Count,
                Uplinks = _acarsMessageService.Uplinks.ToArray()
            };
            foreach (var uplink in result.Uplinks)
            {
                uplink.SelfLink = $"{GetSelflink()}/{uplink.Id}";
            }

            //Get and Set Hash to header response
            HttpContext.Response.Headers.ETag = GetHash(result);

            //if eTag matches, reurn 304.
            if (eTag == HttpContext.Response.Headers.ETag)
            {
                return new StatusCodeResult(StatusCodes.Status304NotModified);
            }

            // if content should be included, return result directly
            if (includeContent)
            {
                //return Ok(result);
                return Ok(result);
            }

            // if content should be exlcuded, remove Data from response
            var buffer = new List<AcarsUplink>();
            foreach (var uplink in result.Uplinks)
            {
                buffer.Add(uplink with { Payload = null });
            }
            result.Uplinks = buffer.ToArray();
            return Ok(result);
        }

        // Get all downlinks
        [HttpGet("downlinks")]
        public async Task<IActionResult> GetDownlinks(
            [FromHeader(Name = "If-None-Match")]string? eTag, 
            [FromQuery(Name = "includeContent")]bool includeContent)
        {
            var result = new AcarsResponseMessageList()
            {
                SelfLink = GetSelflink(),
                Total = _acarsMessageService.Downlinks.Count,
                Start = 0,
                Downlinks = _acarsMessageService.Downlinks.ToArray()
            };

            // Adapt selflink
            foreach (var downlink in result.Downlinks)
            {
                downlink.SelfLink = String.Format($"{GetSelflink()}/{downlink.Id}");
            }
            
            //Get and Set Hash to header response
            HttpContext.Response.Headers.ETag = GetHash(result);
            
            //if eTag matches, reurn 304.
            if (eTag == HttpContext.Response.Headers.ETag)
            {
                return new StatusCodeResult(StatusCodes.Status304NotModified);
            }

            // if content should be included, return result directly
            if (includeContent)
            {
                //return Ok(result);
                return Ok(result);
            }

            // if content should be exlcuded, remove Data from response
            var buffer = new List<AcarsDownlink>();
            foreach (var downlink in result.Downlinks)
            {
                buffer.Add(downlink with { Payload = null});
            }
            result.Downlinks = buffer.ToArray();
            return Ok(result);
        }

        // HACK ON
        [HttpGet("downlinks/envelope")]
        public async Task<IActionResult> GetDownlinksEnveloped(
            [FromHeader(Name = "If-None-Match")] string? eTag,
            [FromQuery(Name = "includeContent")] bool includeContent)
        {
            var result = new AcarsResponseMessageList()
            {
                SelfLink = GetSelflink(),
                Total = _acarsMessageService.Downlinks.Count,
                Start = 0,
                Downlinks = _acarsMessageService.Downlinks.ToArray()

            };

            //Get and Set Hash to header response
            HttpContext.Response.Headers.ETag = GetHash(result);

            //if eTag matches, reurn 304.
            if (eTag == HttpContext.Response.Headers.ETag)
            {
                return new StatusCodeResult(StatusCodes.Status304NotModified);
            }

            // if content should be included, return result directly
            if (includeContent)
            {
                //return Ok(result);
                return Ok(new AcarsEnvelope() { message = result, type = result.GetType().ToString() });
            }

            // if content should be exlcuded, remove Data from response
            var buffer = new List<AcarsDownlink>();
            foreach (var downlink in result.Downlinks)
            {
                buffer.Add(downlink with { Payload = null });
            }
            result.Downlinks = buffer.ToArray();
            return Ok(new AcarsEnvelope() { message = result, type = result.GetType().ToString() });

        }
        // HACK OFF

        // Get single uplink
        [HttpGet("uplinks/{id}")]
        public async Task<IActionResult> GetUplink(Guid id)
        {
            var result = _acarsMessageService.Uplinks.Find(ul => ul.Id == id);
            if (result != null)
            {
                result.SelfLink = $"{GetSelflink()}/{result.Id}";
                return Ok(result);
            }

            return NotFound();
        }

        // Get single downlink
        [HttpGet("downlinks/{id}")]
        public async Task<IActionResult> GetDownlink(Guid id)
        {
            var result = _acarsMessageService.Downlinks.Find(ul => ul.Id == id);
            if (result != null)
            {
                result.SelfLink = $"{GetSelflink()}/{result.Id}";
                return Ok(result);
            }

            return NotFound();
        }

        [HttpPost("downlinks")]
        public async Task<IActionResult> SendDownlink([FromBody] AcarsDownlinkRequest downlinkRequest)
        {
            if (!downlinkRequest.Validate())
            {
                return BadRequest();
            }

            var dlResult = _acarsMessageService.SendDownlink(downlinkRequest);
            dlResult.SelfLink = $"{GetSelflink()}/{dlResult.Id}";
            if (dlResult is not null)
            {
                return Ok(dlResult);
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpDelete("uplinks")]
        public async Task<IActionResult> DeleteUplinks()
        {
            _acarsMessageService.DeleteUplinks();            
            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpDelete("uplinks/{id}")]
        public async Task<IActionResult> DeleteUplink(Guid id)
        {
            if (_acarsMessageService.DeleteUplink(id))
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return NotFound();
        }

        [HttpDelete("downlinks")]
        public async Task<IActionResult> DeleteDownlinks()
        {
            _acarsMessageService.DeleteDownlinks();
            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpDelete("downlinks/{id}")]
        public async Task<IActionResult> DeleteDownlink(Guid id)
        {
            if (_acarsMessageService.DeleteDownlink(id))
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return NotFound();
        }

        [HttpGet("subscribe")]        
        public async Task<IActionResult> HandleWebsocket()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var protocols = HttpContext.WebSockets.WebSocketRequestedProtocols;
                System.Net.WebSockets.WebSocket webSocket = null;

                if (protocols.Count > 0)
                {
                    if (protocols.Contains("acars-1"))
                    {
                        // Accept the WebSocket connection and return the client that 'acars-1' subprotocol is used.
                        webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync("acars-1");
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
                var client = _serviceProvider.GetService<IWebSocketClientHandlerAcars>();

                // Handover the established websocket connection to the client handlet and start it
                await client.StartListen(webSocket, HttpContext);

                // If the clientHandler finishes (due to cloes websocket connection)
                // it will end up here and we tidy up everything.
                client.Dispose();
                return new EmptyResult();
            }
            else
            {
                // If the request was not a valid WebSocket request, return BadRequest
                return BadRequest("Resource can only be accessed with WebSockets.");                
            }
        }

        private string GetSelflink()
        {
            return $"{Request.Scheme}://{Request.Host.Value}{Request.Path}";
        }

        private IActionResult CacheResult(string eTag, object result, bool exludeData = false)
        {
            HttpContext.Response.Headers.ETag = GetHash(result);
            if (eTag == HttpContext.Response.Headers.ETag)
            {
                return new StatusCodeResult(StatusCodes.Status304NotModified);
            }

            return Ok(result);
        }

        private string GetHash(object testObject)
        {
            var teststring = JsonSerializer.Serialize(testObject);
            return teststring.GetHashCode().ToString("X");

        }
    }
}
