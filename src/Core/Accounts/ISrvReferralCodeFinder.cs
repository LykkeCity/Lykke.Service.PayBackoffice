using System.Threading.Tasks;

namespace Core.Accounts
{
    public interface ISrvReferralCodeFinder
    {
        /// <summary>
        /// Searches for referral code by client ip.
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <returns>Referral code,, if client registered using ref link. Null otherwise</returns>
        Task<string> FindCode(string ip);
    }
}
