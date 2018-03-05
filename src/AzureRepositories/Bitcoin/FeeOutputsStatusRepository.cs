using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using Core.BitCoin;

namespace AzureRepositories.Bitcoin
{
    public class FeeOutputsStatusRepository : IFeeOutputsStatusRepository
    {
        private readonly IBlobStorage _blobStorage;
        private const string ContainerName = "info";
        private const string FileName = "feeOutputsStatus.json";

        public FeeOutputsStatusRepository(IBlobStorage blobStorage)
        {
            _blobStorage = blobStorage;
        }

        public async Task AddOrUpdate(string feeOutputsStatusJson)
        {
            await _blobStorage.SaveBlobAsync(ContainerName, FileName, Encoding.UTF8.GetBytes(feeOutputsStatusJson));
        }

        public async Task<string> GetAsync()
        {
            return await _blobStorage.GetAsTextAsync(ContainerName, FileName);
        }
    }
}
