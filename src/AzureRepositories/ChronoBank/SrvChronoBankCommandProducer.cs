﻿using System.Threading.Tasks;
using AzureStorage.Queue;
using Common;
using Core.ChronoBank;

namespace AzureRepositories.ChronoBank
{
    public class SrvChronoBankCommandProducer : IChronoBankCommandProducer
    {
        private readonly IQueueExt _queueExt;

        public SrvChronoBankCommandProducer(IQueueExt queueExt)
        {
            _queueExt = queueExt;
        }

        public async Task ProduceCashOutCommand(string id, string addressTo, double amount)
        {
            await _queueExt.PutRawMessageAsync(new ChronoBankCashOutCommand
            {
                Id = id,
                Amount = amount,
                Address = addressTo
            }.ToJson());
        }

        public class ChronoBankCashOutCommand
        {
            public string Id { get; set; }
            public string Address { get; set; }
            public double Amount { get; set; }
        }
    }
}
