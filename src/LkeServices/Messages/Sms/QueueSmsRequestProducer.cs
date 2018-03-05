using System.Threading.Tasks;
using Core.Messages.Sms;
using Core.Sms;

namespace LkeServices.Messages.Sms
{
    public class QueueSmsRequestProducer : ISmsRequestProducer
    {
        private readonly ISmsCommandProducer _smsCommandProducer;

        public QueueSmsRequestProducer(ISmsCommandProducer smsCommandProducer)
        {
            _smsCommandProducer = smsCommandProducer;
        }

        public async Task SendSmsAsync<T>(string partnerId, string phoneNumber, T msgData, bool useAlternativeProvider)
        {
            await _smsCommandProducer.ProduceSendSmsCommand(partnerId, phoneNumber, msgData, useAlternativeProvider);
        }
    }
}
