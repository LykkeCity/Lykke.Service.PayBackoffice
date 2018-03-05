using System;
using System.Threading.Tasks;
using Core.Ico;
using Core.Settings;
using Flurl.Http;

namespace LkeServices.Ico
{
    public class IcoWhitelistService : IIcoWhitelistService
    {
        private readonly IcoSettings _icoSettings;

        public IcoWhitelistService(IcoSettings icoSettings)
        {
            _icoSettings = icoSettings;
        }

        public async Task<bool> IsUserWhitelisted(string email)
        {
            var endpoint = _icoSettings.CheckWhitelistedUrl.EndsWith('/')
                ? $"{_icoSettings.CheckWhitelistedUrl}{email}"
                : $"{_icoSettings.CheckWhitelistedUrl}/{email}";

            return Convert.ToBoolean(await endpoint.GetStringAsync());
        }
    }
}
