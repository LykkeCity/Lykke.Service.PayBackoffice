using System.Threading.Tasks;

namespace Core.Messages.Sms
{
    public interface ISmsSender
    {
        string GetSenderNumber(string recipientNumber);
        Task ProcessSmsAsync(string phoneNumber, SmsMessage message);
    }

    public interface IAlternativeSmsSender : ISmsSender { }
}
