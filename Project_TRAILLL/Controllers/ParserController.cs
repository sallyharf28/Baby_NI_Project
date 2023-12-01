using Microsoft.AspNetCore.Mvc;
using Project_TRAILLL.Services;

namespace Project_TRAILLL.Controllers
{      
        [ApiController]
        [Route("api/parser")]
    public class ParserController :  ControllerBase
    {
        
        private readonly IParserService _parserservice;

        public ParserController(IParserService parserservice)
        {
            _parserservice = parserservice;
        }
 
    }

}
