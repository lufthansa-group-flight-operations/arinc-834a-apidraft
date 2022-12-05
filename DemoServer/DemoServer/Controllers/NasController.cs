using Microsoft.AspNetCore.Mvc;

namespace DemoServer.Controllers
{
    [Route("a834a/nas/v1")]
    [ApiController]
    public class NasController : ControllerBase
    {
        [HttpGet]
        [HttpGet("{resource?}")]
        public async Task<IActionResult> NotImplemented(string resource = "")
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }
    }
}
