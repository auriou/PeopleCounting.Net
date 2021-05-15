using System.Threading.Tasks;

namespace HubObjectCounting.Hub
{
    public interface ICounterClient
    {
        Task SendCount(int count);
    }
}