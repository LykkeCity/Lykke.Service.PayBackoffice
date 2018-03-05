using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Core.Messages.Sms;
using Core.Sms;
using LkeServices.Messages.Settings;

namespace LkeServices.Messages.Sms
{
    //https://docs.nexmo.com/messaging/sms-api/api-reference#keys
    public enum NexmoStatusCode
    {
        Success = 0,
        Throttled = 1,
        MissingParams = 2,
        InvalidParams = 3,
        InvalidCredentials = 4,
        InternalError = 5,
        InvalidMessage = 6,
        NumberBarred = 7,
        PartnerAccountBarred = 8,
        PartnerQuotaExceeded = 9,
        AccountNotEnabledForRest = 11,
        MessageTooLong = 12,
        CommunicationFailed = 13,
        InvalidSignature = 14,
        InvalidSenderAddress = 15,
        InvalidTTL = 16,
        FacilityNotAllowed = 19,
        InvalidMessageClass = 20,
        BadCallback = 23,
        NonWhiteListedDestination = 29
    }

    public class NexmoResponse
    {
        public class NexmoMessage
        {
            public NexmoStatusCode Status { get; set; }
            public string MessageId { get; set; }
            public string To { get; set; }
            public string ClientRef { get; set; }
            public string RemainingBalance { get; set; }
            public string MessagePrice { get; set; }
            public string Network { get; set; }
            public string ErrorText { get; set; }
        }

        public int MessageCount { get; set; }
        public IEnumerable<NexmoMessage> Messages { get; set; } 
    }

    public class NexmoSmsSender : ISmsSender
    {
        private readonly SmsSettings _settings;
        private readonly ILog _log;

        private const string NexmoSendSmsUrlFormat =
            "https://rest.nexmo.com/sms/json?api_key={0}&api_secret={1}&from={2}&to={3}&text={4}";

        public NexmoSmsSender(SmsSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        public string GetSenderNumber(string recipientNumber)
        {
            return recipientNumber.IsUSCanadaNumber() ? _settings.UsCanadaNumber : _settings.OtherSender;
        }

        public async Task ProcessSmsAsync(string phoneNumber, SmsMessage message)
        {
            string urlEncodedText = message.Text.EncodeUrl();
            var url = string.Format(NexmoSendSmsUrlFormat, _settings.NexmoAppKey, _settings.NexmoAppSecret, message.From,
                phoneNumber, urlEncodedText);
            var client = new HttpClient();
            var response = await client.GetAsync(url);

            HttpContent responseContent = response.Content;
            NexmoResponse responseObj = null;
            string responseString;

            using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
            {
                responseString = await reader.ReadToEndAsync();
                responseObj = responseString.DeserializeJson<NexmoResponse>();
            }

            if (responseObj != null)
            {
                foreach (var msg in responseObj.Messages)
                {
                    if (msg.Status != NexmoStatusCode.Success)
                    {
                        await _log.WriteWarningAsync("NexmoSMS", "ProcessSms", responseString, "SMS was not sent", DateTime.UtcNow);
                    }
                }
            }
        }
    }
}
