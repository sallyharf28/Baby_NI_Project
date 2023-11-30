﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Retreivedata.Models;
using Retreivedata.Service;

namespace Retreivedata.Controllers
{
    [Route("api/fetch")]
    [ApiController]
    public class FetchController : ControllerBase
    {
        private readonly IFetchService _fetchService;

        public FetchController(IFetchService fetchService)
        {
            _fetchService = fetchService;
            _fetchService.connectionString = "Server=10.10.4.231;Database=test;User=bootcamp4;Password=bootcamp42023";
            Console.WriteLine("im in");
        }

        /*
        public IActionResult Index()
        {
            var hourlyData = _fetchService.GetHourlyAggregatedData();
            var dailyData = _fetchService.GetDailyAggregatedData();
            List<string> aggregatedData = new List<string>();
            aggregatedData.Add(hourlyData.ToString());
            aggregatedData.Add(dailyData.ToString());
            Console.WriteLine(aggregatedData.Count);
            return Ok(aggregatedData);
        }
        */

        //[HttpGet("hourly")]
        //public ActionResult<List<AggregatedData>> GetHourlyAggregatedData(bool ne)
        //{
        //    var hourlyData = _fetchService.GetHourlyAggregatedData();
        //    return Ok(hourlyData);
        //}



        //[HttpGet("daily")]
        //public ActionResult<List<DailyAggregatedData>> GetDailyAggregatedData()
        //{

        [HttpGet("allAggregatedData")]
        public ActionResult<List<AggregatedData>> GetAllAggregatedData()
        {
                var allData = _fetchService.GetAllAggregatedData();
                return Ok(allData);         
        }

    }
}
