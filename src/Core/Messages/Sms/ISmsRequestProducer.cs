using System.Threading.Tasks;

namespace Core.Messages.Sms
{
    public interface ISmsRequestProducer
    {
        Task SendSmsAsync<T>(string partnerId, string phoneNumber, T msgData, bool useAlternativeProvider);
    }
}
