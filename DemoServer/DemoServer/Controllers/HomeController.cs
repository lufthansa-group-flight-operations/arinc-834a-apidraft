//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;

namespace DemoServer.Controllers
{
    [Route("")]
    [ApiController]
    [Produces("text/plain")]
    public class HomeController : ControllerBase

    {
        private readonly ILogger _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("")]
        [OpenApiOperation("Get")]
        public virtual IActionResult Get()
        {
            return Ok(@"ARINC 834A Proof of Concept Server");
        }


        [HttpGet]
        [Route("top")]
        [OpenApiOperation("GetTop")]
        public virtual IActionResult GetTop()
        {
            try
            {
                var info = System.IO.File.ReadAllText("/tmp/data/top.txt");
                return Ok(info);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, @"Currently no system information available");
            }
        }

        [HttpGet]
        [Route("flightplan")]
        [OpenApiOperation("GetFlightPlan")]
        public virtual IActionResult GetFlightPlan()
        {
            try
            {
                var info = System.IO.File.ReadAllText(Path.GetFullPath(Path.Combine("tmp", "a702.txt")));
                return Ok(info);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, @"Currently no flightplan available");
            }
        }

        [HttpPut]
        [Route("flightplan")]
        [RequestSizeLimit(524288000)]
        [OpenApiOperation("SetFlightPlan")]
        public virtual async Task<IActionResult> SetFlightPlanAsync()
        {
            try
            {
                FileStream DestinationStream = System.IO.File.Create(Path.GetFullPath(Path.Combine("tmp", "a702.txt")));
                await Request.Body.CopyToAsync(DestinationStream);
                DestinationStream.Close();
                return Ok("Fightlan is set!");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, @"Can't set flightplan");
            }
        }

        [HttpGet]
        [Route("cdss")]
        [OpenApiOperation("GetVideoStream")]
        public virtual IActionResult GetVideoStream()
        {
            return PhysicalFile(
                @"C:\Users\U715956\Documents\Programming\A834AApiDraft\DemoServer\DemoServer\Media\cdss.mp4",
                "application/octet-stream",
                enableRangeProcessing: true);
        }
    }
}