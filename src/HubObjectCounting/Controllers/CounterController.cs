using HubObjectCounting.Hub;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace HubObjectCounting.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CounterController : ControllerBase
    {
        readonly IHubContext<HubCounter, ICounterClient> _hubContext;
        public CounterController(IHubContext<HubCounter, ICounterClient> hubContext)
        {
            _hubContext = hubContext;
        }
        
        [HttpGet("{count}")]
        public void Get(int count)
        {
            _hubContext.Clients.All.SendCount(5);
        }
    }
}
