﻿using System.Threading.Tasks;
using AzureStorage.Queue;
using Common;
using Core.Quanta;

namespace AzureRepositories.Quanta
{
    public class SrvQuantaCommandProducer : IQuantaCommandProducer
    {
        private readonly IQueueExt _queueExt;

        public SrvQuantaCommandProducer(IQueueExt queueExt)
        {
            _queueExt = queueExt;
        }

        public async Task ProduceCashOutCommand(string id, string addressTo, double amount)
        {
            await _queueExt.PutRawMessageAsync(new QuantaCashOutCommand
            {
                Id = id,
                Amount = amount,
                Address = addressTo
            }.ToJson());
        }

        public class QuantaCashOutCommand
        {
            public string Id { get; set; }
            public string Address { get; set; }
            public double Amount { get; set; }
        }
    }
}
