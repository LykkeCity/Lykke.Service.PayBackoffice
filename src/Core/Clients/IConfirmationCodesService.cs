using System.Threading.Tasks;

namespace Core.Clients
{
    public interface IConfirmationCodesService
    {
        Task<string> RequestSmsCode(string partnerId, string clientId, string phoneNumber, bool createPriorityCode = false);
        Task<string> RequestSmsCode(string partnerId, string phoneNumber, bool createPriorityCode = false);
    }
}
