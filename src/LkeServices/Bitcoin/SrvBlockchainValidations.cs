using System.Linq;
using System.Threading.Tasks;
using Core.Bitcoin;
using Core.BitCoin;

namespace LkeServices.Bitcoin
{
    public enum ValidationErrors
    {
        None,
        InvalidAddress,
        ColoredAddressExpected,
        SameSourceAsDestination
    }

    public class SrvBlockchainValidations
    {
        private readonly ISrvBlockchainReader _srvBlockchainReader;
        private readonly IWalletCredentialsRepository _walletCredentialsRepository;

        public SrvBlockchainValidations(ISrvBlockchainReader srvBlockchainReader,
            IWalletCredentialsRepository walletCredentialsRepository)
        {
            _srvBlockchainReader = srvBlockchainReader;
            _walletCredentialsRepository = walletCredentialsRepository;
        }

        public async Task<ValidationErrors> IsValidAddressToCashout(string clientId, bool isColoredCashOut, string destinationAddress)
        {
            var walletCreds = await _walletCredentialsRepository.GetAsync(clientId);

            if (!await _srvBlockchainReader.IsValidAddress(destinationAddress, enableColored: true))
                return ValidationErrors.InvalidAddress;

            if (isColoredCashOut && !await _srvBlockchainReader.IsColoredAddress(destinationAddress))
                return ValidationErrors.ColoredAddressExpected;

            if (await _srvBlockchainReader.GetUncoloredAdress(destinationAddress) == walletCreds.MultiSig)
                return ValidationErrors.SameSourceAsDestination;

            return ValidationErrors.None;
        }
    }
}
