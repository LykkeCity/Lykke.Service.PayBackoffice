// Code generated by Microsoft (R) AutoRest Code Generator 0.17.0.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace LkeServices.Generated.ClientSigningApi
{
    using System.Threading.Tasks;
   using Models;

    /// <summary>
    /// Extension methods for LykkeSigningAPI.
    /// </summary>
    public static partial class LykkeSigningAPIExtensions
    {
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            public static PubKeyResponse ApiBitcoinKeyGet(this ILykkeSigningAPI operations)
            {
                return System.Threading.Tasks.Task.Factory.StartNew(s => ((ILykkeSigningAPI)s).ApiBitcoinKeyGetAsync(), operations, System.Threading.CancellationToken.None, System.Threading.Tasks.TaskCreationOptions.None, System.Threading.Tasks.TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async System.Threading.Tasks.Task<PubKeyResponse> ApiBitcoinKeyGetAsync(this ILykkeSigningAPI operations, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
            {
                using (var _result = await operations.ApiBitcoinKeyGetWithHttpMessagesAsync(null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            public static void ApiBitcoinTempKeyGet(this ILykkeSigningAPI operations)
            {
                System.Threading.Tasks.Task.Factory.StartNew(s => ((ILykkeSigningAPI)s).ApiBitcoinTempKeyGetAsync(), operations, System.Threading.CancellationToken.None, System.Threading.Tasks.TaskCreationOptions.None,  System.Threading.Tasks.TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async System.Threading.Tasks.Task ApiBitcoinTempKeyGetAsync(this ILykkeSigningAPI operations, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
            {
                await operations.ApiBitcoinTempKeyGetWithHttpMessagesAsync(null, cancellationToken).ConfigureAwait(false);
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='model'>
            /// </param>
            public static void ApiBitcoinAddkeyPost(this ILykkeSigningAPI operations, AddKeyRequest model = default(AddKeyRequest))
            {
                System.Threading.Tasks.Task.Factory.StartNew(s => ((ILykkeSigningAPI)s).ApiBitcoinAddkeyPostAsync(model), operations, System.Threading.CancellationToken.None, System.Threading.Tasks.TaskCreationOptions.None,  System.Threading.Tasks.TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='model'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async System.Threading.Tasks.Task ApiBitcoinAddkeyPostAsync(this ILykkeSigningAPI operations, AddKeyRequest model = default(AddKeyRequest), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
            {
                await operations.ApiBitcoinAddkeyPostWithHttpMessagesAsync(model, null, cancellationToken).ConfigureAwait(false);
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='signRequest'>
            /// </param>
            public static TransactionSignResponse ApiBitcoinSignPost(this ILykkeSigningAPI operations, BitcoinTransactionSignRequest signRequest = default(BitcoinTransactionSignRequest))
            {
                return System.Threading.Tasks.Task.Factory.StartNew(s => ((ILykkeSigningAPI)s).ApiBitcoinSignPostAsync(signRequest), operations, System.Threading.CancellationToken.None, System.Threading.Tasks.TaskCreationOptions.None, System.Threading.Tasks.TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='signRequest'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async System.Threading.Tasks.Task<TransactionSignResponse> ApiBitcoinSignPostAsync(this ILykkeSigningAPI operations, BitcoinTransactionSignRequest signRequest = default(BitcoinTransactionSignRequest), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
            {
                using (var _result = await operations.ApiBitcoinSignPostWithHttpMessagesAsync(signRequest, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='address'>
            /// </param>
            public static PrivateKeyResponse ApiBitcoinGetkeyGet(this ILykkeSigningAPI operations, string address = default(string))
            {
                return System.Threading.Tasks.Task.Factory.StartNew(s => ((ILykkeSigningAPI)s).ApiBitcoinGetkeyGetAsync(address), operations, System.Threading.CancellationToken.None, System.Threading.Tasks.TaskCreationOptions.None, System.Threading.Tasks.TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='address'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async System.Threading.Tasks.Task<PrivateKeyResponse> ApiBitcoinGetkeyGetAsync(this ILykkeSigningAPI operations, string address = default(string), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
            {
                using (var _result = await operations.ApiBitcoinGetkeyGetWithHttpMessagesAsync(address, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            public static EthereumAddressResponse ApiEthereumKeyGet(this ILykkeSigningAPI operations)
            {
                return System.Threading.Tasks.Task.Factory.StartNew(s => ((ILykkeSigningAPI)s).ApiEthereumKeyGetAsync(), operations, System.Threading.CancellationToken.None, System.Threading.Tasks.TaskCreationOptions.None, System.Threading.Tasks.TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async System.Threading.Tasks.Task<EthereumAddressResponse> ApiEthereumKeyGetAsync(this ILykkeSigningAPI operations, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
            {
                using (var _result = await operations.ApiEthereumKeyGetWithHttpMessagesAsync(null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='model'>
            /// </param>
            public static void ApiEthereumAddkeyPost(this ILykkeSigningAPI operations, AddKeyRequest model = default(AddKeyRequest))
            {
                System.Threading.Tasks.Task.Factory.StartNew(s => ((ILykkeSigningAPI)s).ApiEthereumAddkeyPostAsync(model), operations, System.Threading.CancellationToken.None, System.Threading.Tasks.TaskCreationOptions.None,  System.Threading.Tasks.TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='model'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async System.Threading.Tasks.Task ApiEthereumAddkeyPostAsync(this ILykkeSigningAPI operations, AddKeyRequest model = default(AddKeyRequest), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
            {
                await operations.ApiEthereumAddkeyPostWithHttpMessagesAsync(model, null, cancellationToken).ConfigureAwait(false);
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='signRequest'>
            /// </param>
            public static TransactionSignResponse ApiEthereumSignPost(this ILykkeSigningAPI operations, EthereumTransactionSignRequest signRequest = default(EthereumTransactionSignRequest))
            {
                return System.Threading.Tasks.Task.Factory.StartNew(s => ((ILykkeSigningAPI)s).ApiEthereumSignPostAsync(signRequest), operations, System.Threading.CancellationToken.None, System.Threading.Tasks.TaskCreationOptions.None, System.Threading.Tasks.TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='signRequest'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async System.Threading.Tasks.Task<TransactionSignResponse> ApiEthereumSignPostAsync(this ILykkeSigningAPI operations, EthereumTransactionSignRequest signRequest = default(EthereumTransactionSignRequest), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
            {
                using (var _result = await operations.ApiEthereumSignPostWithHttpMessagesAsync(signRequest, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            public static IsAliveResponse ApiIsAliveGet(this ILykkeSigningAPI operations)
            {
                return System.Threading.Tasks.Task.Factory.StartNew(s => ((ILykkeSigningAPI)s).ApiIsAliveGetAsync(), operations, System.Threading.CancellationToken.None, System.Threading.Tasks.TaskCreationOptions.None, System.Threading.Tasks.TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async System.Threading.Tasks.Task<IsAliveResponse> ApiIsAliveGetAsync(this ILykkeSigningAPI operations, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
            {
                using (var _result = await operations.ApiIsAliveGetWithHttpMessagesAsync(null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

    }
}
