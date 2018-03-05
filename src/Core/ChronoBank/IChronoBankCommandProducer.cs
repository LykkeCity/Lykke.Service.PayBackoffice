using System.Threading.Tasks;

namespace Core.ChronoBank
{
    public interface IChronoBankCommandProducer
    {
        Task ProduceCashOutCommand(string id, string addressTo, double amount);
    }
}
