using Microsoft.AspNetCore.Mvc;
using Project_TRAILLL.Services;

namespace Project_TRAILLL.Controllers
{
   
    [ApiController]
    [Route("api/aggregate")]
    public class AggregationController :ControllerBase
    {
        private readonly IAggregationService _aggregationService;

        public AggregationController(IAggregationService aggregationService) 
        { 
              _aggregationService = aggregationService;
        }


        [HttpPost("hourly")]
        public IActionResult AggregateHourlyData()
        { 
            _aggregationService.AggragateHourlyData();
            return Ok("Hourly aggregation triggered successfully.");
           
        }

        [HttpPost("daily")]
        public IActionResult AggregateDailyData()
        {
           _aggregationService.AggregateDailyData();
            return Ok("Daily aggregation triggered successfully.");
            
        }
    }
}
