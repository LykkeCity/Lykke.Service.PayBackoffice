using System.Threading.Tasks;
using Core.Messages.Sms;

namespace LkeServices.Messages.Sms
{
    public class SmsMockSender : ISmsSender
    {
        private readonly ISmsMockRepository _smsMockRepository;

        public SmsMockSender(ISmsMockRepository smsMockRepository)
        {
            _smsMockRepository = smsMockRepository;
        }

        public string GetSenderNumber(string recipientNumber)
        {
            return "SmsMockSender";
        }

        public Task ProcessSmsAsync(string phoneNumber, SmsMessage message)
        {
            return _smsMockRepository.InsertAsync(phoneNumber, message);
        }
    }

    public class AlternativeSmsMockSender : IAlternativeSmsSender
    {
        private readonly ISmsMockRepository _smsMockRepository;

        public AlternativeSmsMockSender(ISmsMockRepository smsMockRepository)
        {
            _smsMockRepository = smsMockRepository;
        }

        public string GetSenderNumber(string recipientNumber)
        {
            return "AlternativeSmsMockSender";
        }

        public Task ProcessSmsAsync(string phoneNumber, SmsMessage message)
        {
            return _smsMockRepository.InsertAsync(phoneNumber, message);
        }
    }
}
