using System;
using System.Collections.Generic;

namespace Core.PaymentSystems
{
    public enum CashInPaymentSystem
    {
        Unknown, CreditVoucher, Bitcoin, Ethereum, Swift, SolarCoin, ChronoBank, Fxpaygate, Quanta
    }

    public enum CardPaymentSystem
    {
        Unknown, CreditVoucher, Fxpaygate
    }

    public static class PaymentSystemsAndOtherInfo
    {
    
        public static readonly Dictionary<CashInPaymentSystem, Type> PsAndOtherInfoLinks = new Dictionary<CashInPaymentSystem, Type>
        {
            [CashInPaymentSystem.CreditVoucher] = typeof(OtherPaymentInfo),
            [CashInPaymentSystem.Fxpaygate] = typeof(OtherPaymentInfo),
        };
    }
}
