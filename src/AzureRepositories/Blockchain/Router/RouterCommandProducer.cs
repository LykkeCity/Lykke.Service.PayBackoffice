using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage.Queue;
using Common;
using Core.Blockchain.Router;

namespace AzureRepositories.Blockchain.Router
{
    public class RouterCommandProducer : IRouterCommandProducer
    {
        private readonly IQueueExt _queueExt;

        public RouterCommandProducer(IQueueExt queueExt)
        {
            _queueExt = queueExt;
        }

        public Task ProduceCashOut(string addressFrom, string addressTo, double amount, string assetId)
        {
            var request = new Request
            {
                Action = ActionType.CashOut,
                Parameters = new Dictionary<string, string>
                {
                    {CommandsKeys.Asset, assetId},
                    {CommandsKeys.Amount, amount.ToString()},
                    {CommandsKeys.MultisigAddress, addressFrom},
                    {CommandsKeys.To, addressTo }
                }
            };

            return _queueExt.PutRawMessageAsync(request.ToJson());
        }
    }
}
