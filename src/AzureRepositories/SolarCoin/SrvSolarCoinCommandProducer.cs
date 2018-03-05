﻿using System.Threading.Tasks;
using AzureStorage.Queue;
using Common;
using Core.SolarCoin;

namespace AzureRepositories.SolarCoin
{
    public class SrvSolarCoinCommandProducer : ISrvSolarCoinCommandProducer
    {
        private readonly IQueueExt _queueExt;

        public SrvSolarCoinCommandProducer(IQueueExt queueExt)
        {
            _queueExt = queueExt;
        }

        public async Task ProduceCashOutCommand(string id, SolarCoinAddress addressTo, double amount)
        {
            await _queueExt.PutRawMessageAsync(new SolarCashOutCommand
            {
                Id = id,
                Amount = amount,
                Address = addressTo.Value
            }.ToJson());
        }

        public class SolarCashOutCommand
        {
            public string Id { get; set; }
            public string Address { get; set; }
            public double Amount { get; set; }
        }
    }
}
