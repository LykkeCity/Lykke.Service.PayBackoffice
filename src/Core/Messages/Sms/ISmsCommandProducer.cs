using System.Threading.Tasks;

namespace Core.Messages.Sms
{
    public interface ISmsCommandProducer
    {
        Task ProduceSendSmsCommand<T>(string partnerId, string phoneNumber, T msgData, bool useAlternativeProvider);
    }
}
