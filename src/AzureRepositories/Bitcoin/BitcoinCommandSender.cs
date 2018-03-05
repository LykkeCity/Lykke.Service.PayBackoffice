﻿using System.Threading.Tasks;
using AzureStorage.Queue;
using Common;
using Core.BitCoin;

namespace AzureRepositories.Bitcoin
{
    public class BitcoinCommandSender : IBitcoinCommandSender
    {
        private readonly IQueueExt _queueExt;

        public BitcoinCommandSender(IQueueExt queueExt)
        {
            _queueExt = queueExt;
        }

        public Task SendCommand(BaseCommand command)
        {
            return _queueExt.PutRawMessageAsync(command.ToJson());
        }
    }
}
