using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace HubObjectCounting.Hub
{
    public class HubCounter : Hub<ICounterClient>
    {
        public async Task SendMessage(int count)
        {
            await Clients.All.SendCount(count);
        }
    }
}
