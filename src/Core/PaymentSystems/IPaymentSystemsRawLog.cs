using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.PaymentSystems
{

    public interface IPaymentSystemRawLogEvent
    {
        DateTime DateTime { get; }
        string PaymentSystem { get; }

        string EventType { get; }
        string Data { get; }

    }

    public class PaymentSystemRawLogEvent : IPaymentSystemRawLogEvent
    {
        public DateTime DateTime { get; set; }
        public string PaymentSystem { get; set; }
        public string EventType { get; set; }
        public string Data { get; set; }

        public static PaymentSystemRawLogEvent Create(CashInPaymentSystem paymentSystem, string eventType, string data)
        {
            return new PaymentSystemRawLogEvent
            {
                DateTime = DateTime.UtcNow,
                PaymentSystem = paymentSystem.ToString(),
                Data = data,
                EventType = eventType
            };
        }

    }

    public interface IPaymentSystemsRawLog
    {
        Task RegisterEventAsync(IPaymentSystemRawLogEvent evnt);

    }
}
