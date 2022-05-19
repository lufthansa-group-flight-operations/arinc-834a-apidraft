using DemoServer.DataAccess;
using DemoServer.Models;
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

        public AcarsController(IAcarsMessageService acarsMessageService)
        {
            this._acarsMessageService = acarsMessageService;
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
        public async Task<IActionResult> GetUplinks([FromHeader(Name = "If-None-Match")] string? eTag)
        {
            var result = new AcarsResponseMessageList()
            {
                SelfLink = GetSelflink(),
                Total = _acarsMessageService.Uplinks.Count,
                Uplinks = _acarsMessageService.Uplinks.ToArray()
            };            

            return CacheResult(eTag, result);
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
                return Ok(result);
            }

            // if content should be exlcuded, remove Data from response
            var buffer = new List<AcarsDownlink>();
            foreach (var downlink in result.Downlinks)
            {
                buffer.Add(downlink with { Data = null});
            }
            result.Downlinks = buffer.ToArray();
            return Ok(result);
        }

        // Get single uplink
        [HttpGet("uplinks/{id}")]
        public async Task<IActionResult> GetUplink(int id)
        {
            var result = _acarsMessageService.Uplinks.Find(ul => ul.Id == id);
            if (result != null)
            {
                return Ok(result);
            }

            return NotFound();
        }

        // Get single downlink
        [HttpGet("downlinks/{id}")]
        public async Task<IActionResult> GetDownlink(int id)
        {
            var result = _acarsMessageService.Downlinks.Find(ul => ul.Id == id);
            if (result != null)
            {
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
        public async Task<IActionResult> DeleteUplink(int id)
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
        public async Task<IActionResult> DeleteDownlink(int id)
        {
            if (_acarsMessageService.DeleteDownlink(id))
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return NotFound();
        }

        private string GetSelflink()
        {
            return $"{Request.Scheme}://{Request.Host.Value}/{Request.Path}";
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
