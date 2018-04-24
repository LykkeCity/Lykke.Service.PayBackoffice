using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BackOffice.Filters;
using Core.Users;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayAuth.Client;
using BackOffice.Controllers;
using BackOffice.Translates;
using BackOffice.Areas.LykkePay.Models;
using QBitNinja.Client;
using QBitNinja.Client.Models;
using NBitcoin;
using MoreLinq;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Core.Settings;

namespace BackOffice.Areas.LykkePay.Controllers
{
    [Authorize]
    [Area("LykkePay")]
    [FilterFeaturesAccess(UserFeatureAccess.MenuAssets)]
    public class BtctransfersController : Controller
    {
        private readonly IPayInternalClient _payInternalClient;
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly IPayAuthClient _payAuthClient;
        private readonly QBitNinjaClient _qBitNinjaClient;
        private readonly LykkePayWalletListSettings _walletlist;
        private const int BatchPieceSize = 15;
        private const string ErrorMessageAnchor = "#errorMessage";

        public BtctransfersController(
            IPayInternalClient payInternalClient, IPayInvoiceClient payInvoiceClient, IPayAuthClient payAuthClient, QBitNinjaClient qBitNinjaClient, LykkePayWalletListSettings walletlist)
        {
            _payInternalClient = payInternalClient;
            _payInvoiceClient = payInvoiceClient;
            _payAuthClient = payAuthClient;
            _qBitNinjaClient = qBitNinjaClient ?? throw new ArgumentNullException(nameof(qBitNinjaClient));
            _walletlist = walletlist;
        }
        public async Task<IActionResult> Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> BtcTransfersPage(string merchant = "")
        {
            var merchants = (await _payInternalClient.GetMerchantsAsync()).ToArray();

            if (!string.IsNullOrEmpty(merchant) && !merchants.Select(x => x.Id).Contains(merchant))
            {
                return this.JsonFailResult(Phrases.InvalidValue, "#merchant");
            }

            if (merchants.Any())
            {
                if (string.IsNullOrEmpty(merchant))
                {
                    merchant = merchants.Select(x => x.Id).First();
                }
            }

            return View(new BtctransfersPageViewModel
            {
                SelectedMerchant = merchant,
                Merchants = merchants
            });
        }
        [HttpPost]
        public async Task<ActionResult> BtcTransfersList(BtctransfersPageViewModel vm)
        {
            if (string.IsNullOrEmpty(vm.SelectedMerchant))
                return this.JsonFailResult(Phrases.FieldShouldNotBeEmpty, "#selectedMerchant");

            var paymentrequests = await _payInternalClient.GetPaymentRequestsAsync(vm.SelectedMerchant);
            var addresses = paymentrequests.Select(p => p.WalletAddress).ToList();
            var transactions = (await GetTransactions(addresses)).ToList();
            var filtered = transactions.Where(t => t.Amount > 0).ToList();
            var list = new List<RequestTransferModel>();
            foreach (var transaction in filtered)
            {
                var request = paymentrequests.FirstOrDefault(p => p.WalletAddress == transaction.WalletAddress);
                var tm = list.FirstOrDefault(r => r.PaymentRequest.WalletAddress == transaction.WalletAddress);
                if (tm != null)
                    tm.Amount += transaction.Amount;
                else
                {
                    tm = new RequestTransferModel();
                    tm.Amount = transaction.Amount;
                    tm.AssetId = transaction.AssetId;
                    tm.PaymentRequest = request;
                    list.Add(tm);
                }
            }
            var viewModel = new BtctransfersListViewModel
            {
                List = list,
                SelectedMerchant = vm.SelectedMerchant
            };
            return View(viewModel);
        }
        [HttpPost]
        public async Task<ActionResult> TransferMoneyDialog(TransferModel model)
        {
            var viewmodel = new TransferMoneyDialogViewModel()
            {
                Wallets = _walletlist,
                SelectedMerchant = model.SelectedMerchant,
                SelectedPaymentRequests = model.SelectedPaymentRequests
            };
            return View(viewmodel);
        }
        [HttpPost]
        public async Task<ActionResult> TransferMoney(TransferMoneyDialogViewModel vm)
        {
            var destinationaddress = _walletlist.Wallets.FirstOrDefault(w => w.Address == vm.SelectedWallet);
            try
            {
                if (vm.SelectedPaymentRequests.Count == 0)
                    return this.JsonFailResult("Error: needs to select payment request", ErrorMessageAnchor);
                var request = new BtcFreeTransferRequest();
                request.DestAddress = destinationaddress.Address;
                var sources = new List<BtcTransferSourceInfo>();
                foreach (var item in vm.SelectedPaymentRequests)
                {
                    var sourceinfo = new BtcTransferSourceInfo();
                    sourceinfo.Address = item.PaymentRequest.WalletAddress;
                    sourceinfo.Amount = Convert.ToDecimal(item.Amount/100000000);
                    sources.Add(sourceinfo);
                }
                request.Sources = sources;
                await _payInternalClient.BtcFreeTransferAsync(request);
                return this.JsonRequestResult("#btcTransfersList", Url.Action("BtcTransfersList"), new BtctransfersPageViewModel() { SelectedMerchant = vm.SelectedMerchant } );
            }
            catch (Exception ex)
            {
                return this.JsonFailResult("Error: " + ex.InnerException.Message, ErrorMessageAnchor);
            }
        }
        private async Task<IEnumerable<BlockchainTransaction>> GetTransactions(IEnumerable<string> addresses)
        {
            var balances = new List<WalletBalanceModel>();

            foreach (var batch in addresses.Batch(BatchPieceSize))
            {
                await Task.WhenAll(batch.Select(address => _qBitNinjaClient.GetBalance(BitcoinAddress.Create(address))
                    .ContinueWith(t =>
                    {
                        lock (balances)
                        {
                            balances.Add(new WalletBalanceModel
                            {
                                WalletAddress = address,
                                Balance = t.Result
                            });
                        }
                    })));
            }
            return balances.SelectMany(x => x.GetTransactions());
        }
        private class WalletBalanceModel
        {
            public string WalletAddress { get; set; }
            public BalanceModel Balance { get; set; }

            public IEnumerable<BlockchainTransaction> GetTransactions()
            {
                return Balance?.Operations?.Select(x => new BlockchainTransaction
                {
                    WalletAddress = WalletAddress,
                    Amount = (double)x.Amount.ToDecimal(MoneyUnit.Satoshi),
                    AssetId = nameof(MoneyUnit.Satoshi),
                    Blockchain = "Bitcoin",
                    Id = x.TransactionId.ToString(),
                    BlockId = x.BlockId?.ToString(),
                    Confirmations = x.Confirmations
                });
            }
        }
    }
}
