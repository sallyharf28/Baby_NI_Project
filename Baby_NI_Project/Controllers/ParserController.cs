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
           _parserservice.ProcessTextFile();
           return Ok("Text file processing initiated successfully.");
         
        }
    }

}
