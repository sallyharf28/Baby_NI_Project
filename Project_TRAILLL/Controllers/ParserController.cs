using Microsoft.AspNetCore.Mvc;
using Project_TRAILLL.Services;

namespace Project_TRAILLL.Controllers
{      
        [ApiController]
        [Route("api/[Controller]")]
    public class ParserController :  ControllerBase
    {
        
        private readonly IParserService _parserservice;

        public ParserController(IParserService parserservice)
        {
            _parserservice = parserservice;
        }


        [HttpPost]
        public IActionResult ProcessTextFiles()
        {
            try
            {
                _parserservice.ProcessTextFile();
                return Ok("Text file processing initiated successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error processing text files: {ex.Message}");
            }
        }
    }

}
