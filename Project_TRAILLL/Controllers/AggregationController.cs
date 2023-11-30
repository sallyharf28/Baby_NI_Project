using Microsoft.AspNetCore.Mvc;
using Project_TRAILLL.Services;

namespace Project_TRAILLL.Controllers
{
   
    [ApiController]
    [Route("api/[controller]")]
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
            try
            {
                _aggregationService.AggragateHourlyData();
                return Ok("Hourly aggregation triggered successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error triggering hourly aggregation: {ex.Message}");
            }
        }

        [HttpPost("daily")]
        public IActionResult AggregateDailyData()
        {
            try
            {
                _aggregationService.AggregateDailyData();
                return Ok("Daily aggregation triggered successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error triggering daily aggregation: {ex.Message}");
            }
        }
    }
}
