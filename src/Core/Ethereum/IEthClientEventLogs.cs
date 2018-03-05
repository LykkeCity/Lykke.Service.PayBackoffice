using System.Threading.Tasks;

namespace Core.Ethereum
{
    public enum Event
    {
        Error
    }

    public interface IEthClientEventLogs
    {
        Task WriteEvent(string clientId, Event eventType, string data);
    }
}
