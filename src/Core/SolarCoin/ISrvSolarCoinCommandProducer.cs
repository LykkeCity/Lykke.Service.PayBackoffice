using System.Threading.Tasks;

namespace Core.SolarCoin
{
    public interface ISrvSolarCoinCommandProducer
    {
        Task ProduceCashOutCommand(string id, SolarCoinAddress addressTo, double amount);
    }
}
