using Microsoft.AspNetCore.Mvc;
using NoMoreLegacy.Domain;
using NoMoreLegacy.Services;

namespace NoMoreLegacy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConversorController : ControllerBase
    {
        private readonly ILogger<ConversorController> _logger;
        private readonly ConversorService _conversorService;

        public ConversorController(ILogger<ConversorController> logger, ConversorService conversorService)
        {
            _logger = logger;
            _conversorService = conversorService;
        }

        [HttpPost("")]
        public async Task<IActionResult> Ask([FromForm] IFormFile file, [FromQuery] SupportedFramework framework)
        {
            var res = await _conversorService.DoConversion(file, framework);

            if (!res.Success)
            {
                return Problem(res.Error?.Message);
            }
            
            var fileBytes = res.Value!;
            var downloadFileName = $"converted_{Path.GetFileNameWithoutExtension(file.FileName)}.zip";

            return File(
                fileBytes,                  
                "application/zip",
                downloadFileName
            );
        }

        [HttpGet("status")]
        public IActionResult Status() => Ok("Up!");
    }

}
