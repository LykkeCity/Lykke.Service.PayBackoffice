using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Blockchain.Router
{

    public interface IRouterCommandProducer
    {
        Task ProduceCashOut(string addressFrom, string addressTo, double amount, string assetId);
    }

    #region Models & Constants

    public class Request
    {
        public ActionType Action { get; set; }

        public Dictionary<string, string> Parameters { get; set; }
    }

    public enum ActionType
    {
        CashIn,
        CashOut,
        Swap,
        OrdinaryCashOut,
        Transfer
    }

    public static class CommandsKeys
    {
        public const string TransactionId = "TransactionId";

        public const string Asset = "Asset";
        public const string Asset1 = "Asset1";
        public const string Asset2 = "Asset2";

        public const string MultisigAddress = "MultisigAddress";
        public const string MultisigAddress1 = "MultisigAddress1";
        public const string MultisigAddress2 = "MultisigAddress2";

        public const string Amount = "Amount";
        public const string Amount1 = "Amount1";
        public const string Amount2 = "Amount2";

        public const string To = "To";
    }

    #endregion
}
