using Microsoft.AspNetCore.Mvc;
using Project_TRAILLL.Services;

namespace Project_TRAILLL.Controllers
{
   
    [ApiController]
    [Route("api/[controller]")]
    public class LoaderController : ControllerBase
    {
        private readonly ILoaderService _loaderService;

        public LoaderController(ILoaderService loaderService)
        {
            _loaderService = loaderService;
        }   
    }
}
