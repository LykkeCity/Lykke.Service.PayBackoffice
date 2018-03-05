using System;
using System.Threading.Tasks;

using Common;
using Common.Log;

using Flurl.Http;

namespace LkeServices {
    public abstract class LykkeServiceProxyBase {
        protected abstract string ServiceUri { get; }

        protected abstract string BaseUrl { get; }

        protected abstract string ApiKey { get; }

        protected abstract ILog Log { get; }

        private IFlurlClient GetClient(string action)
        {
            return $"{ServiceUri}/{BaseUrl}/{action}".WithHeader("api-key", ApiKey);
        }

        protected async Task<TResponse> GetDataAsync<TResponse>(string action)
        {
            try
            {
                return await GetClient(action).GetJsonAsync<TResponse>();
            }
            catch (Exception ex)
            {
                await Log.WriteErrorAsync(GetType().Name, action, "GET", ex);
                throw;
            }
        }

        protected async Task<TResponse> PostDataAsync<TResponse>(string action, object data = null) {
            data = data ?? new object();
            try
            {
                return await GetClient(action).PostJsonAsync(data).ReceiveJson<TResponse>();
            }
            catch (Exception ex)
            {
                await Log.WriteErrorAsync(GetType().Name, action, data.ToJson(), ex);
                throw;
            }
        }

        protected async Task PutDataAsync(string action, object data = null)
        {
            data = data ?? new object();
            try
            {
                await GetClient(action).PutJsonAsync(data);
            }
            catch (Exception ex)
            {
                await Log.WriteErrorAsync(GetType().Name, action, data.ToJson(), ex);
                throw;
            }
        }

        protected async Task<TResponse> DeleteDataAsync<TResponse>(string action)
        {
            try
            {
                return await GetClient(action).DeleteAsync().ReceiveJson<TResponse>();
            }
            catch (Exception ex)
            {
                await Log.WriteErrorAsync(GetType().Name, action, "DELETE", ex);
                throw;
            }
        }
    }
}