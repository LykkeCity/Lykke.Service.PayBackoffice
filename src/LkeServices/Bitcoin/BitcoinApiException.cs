using System;
using Lykke.Bitcoin.Api.Client.BitcoinApi.Models;

namespace LkeServices.Bitcoin
{
    public class BitcoinApiException : Exception
    {
        public ErrorResponse Error { get; }

        public BitcoinApiException(ErrorResponse error) : base(error.Message)
        {
            Error = error;
        }
    }
}
