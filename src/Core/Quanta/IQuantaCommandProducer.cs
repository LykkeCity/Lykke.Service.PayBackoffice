using System.Threading.Tasks;

namespace Core.Quanta
{
    public interface IQuantaCommandProducer
    {
        Task ProduceCashOutCommand(string id, string addressTo, double amount);
    }
}
