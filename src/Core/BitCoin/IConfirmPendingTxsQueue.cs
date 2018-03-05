using System.Threading.Tasks;

namespace Core.BitCoin
{
    public class PendingTxMsg
    {
        public string Hash { get; set; }
    }

    public interface IConfirmPendingTxsQueue
    {
        Task PutAsync(PendingTxMsg msg);
    }
}
