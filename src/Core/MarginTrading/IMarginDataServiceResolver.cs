using MarginTrading.DataReaderClient;

namespace Core.MarginTrading
{
    public interface IMarginDataServiceResolver
    {
        IMarginDataService Resolve(bool isDemo);        
        IMarginTradingDataReaderApiClient GetDataReader(bool isDemo);
    }
}
