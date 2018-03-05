using System.Threading.Tasks;
using AzureStorage.Queue;
using Core.Messages.Sms;
using Core.Sms.MessagesData;

namespace AzureRepositories.Sms
{
    public class SmsCommandProducer : ISmsCommandProducer
    {
        private readonly IQueueExt _queueExt;

        public SmsCommandProducer(IQueueExt queueExt)
        {
            _queueExt = queueExt;

            _queueExt.RegisterTypes(
                QueueType.Create("SmsConfirmMessage", typeof(SendSmsData<SmsConfirmationData>))
                );
            _queueExt.RegisterTypes(
                QueueType.Create("SimpleSmsMessage", typeof(SendSmsData<string>))
                );
        }

        public Task ProduceSendSmsCommand<T>(string partnerId, string phoneNumber, T msgData, bool useAlternativeProvider)
        {
            var msg = new SendSmsData<T>
            {
                PartnerId = partnerId,
                MessageData = msgData,
                PhoneNumber = phoneNumber,
                UseAlternativeProvider = useAlternativeProvider
            };

            return _queueExt.PutMessageAsync(msg);
        }
    }
}
