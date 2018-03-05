using System.Threading.Tasks;
using Core;
using Core.Broadcast;
using Core.Clients;
using Core.Messages.Email;
using Core.PaymentSystems;
using Lykke.Messages.Email;
using Lykke.Service.PersonalData.Contract;

namespace LkeServices.PaymentSystems.PaymentOkNotificators
{
    public class PaymentOkEmailSender : IPaymentOkNotification
    {
        private readonly IEmailSender _emailSender;
        private readonly IPersonalDataService _personalDataService;

        public PaymentOkEmailSender(IEmailSender emailSender, IPersonalDataService personalDataService)
        {
            _emailSender = emailSender;
            _personalDataService = personalDataService;
        }

        public async Task NotifyAsync(IPaymentTransaction pt)
        {
            var pd = await _personalDataService.GetAsync(pt.ClientId);


            var body =
                $"Client: {pd.Email}, Payment system amount: {pt.AssetId} {pt.Amount.MoneyToStr()}, Deposited amount: {pt.DepositedAssetId} {pt.DepositedAmount}, PaymentSystem={pt.PaymentSystem}";

            await
                _emailSender.BroadcastEmailAsync(null, BroadcastGroup.Payments,
                    "Payment notification Ok", body);
        }
    }
}
