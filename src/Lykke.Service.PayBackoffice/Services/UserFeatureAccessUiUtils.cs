using BackOffice.Translates;
using Core.PaymentSystems;
using Core.Users;

namespace BackOffice.Services
{
    public static class UserFeatureAccessUiUtils
    {
        public static string GetCaption(this UserFeatureAccess userFeatureAccess)
        {
            switch (userFeatureAccess)
            {
                case UserFeatureAccess.MenuReports:
                    return Phrases.ViewReports;

                case UserFeatureAccess.MenuAssetPairs:
                    return "Access to asset pairs menu";


                case UserFeatureAccess.MenuAssets:
                    return "Access to assets menu";

                case UserFeatureAccess.MenuBroadcast:
                    return "Access to broadcast menu";

                case UserFeatureAccess.MenuClients:
                    return "Access to clients menu";

                case UserFeatureAccess.MenuAddressBook:
                    return "Access address book";

                case UserFeatureAccess.MenuFeeRecharging:
                    return "Access to fee recharging menu";

                case UserFeatureAccess.MenuIssuers:
                    return "Access to issuers menu";

                case UserFeatureAccess.MenuKyc:
                    return "Access to Kyc menu";

                case UserFeatureAccess.MenuOrderBook:
                    return "Access to Orderbook menu";

                case UserFeatureAccess.MenuPayments:
                    return "Access to Payments menu";

                case UserFeatureAccess.RemoteUi:
                    return "Access to monitor menu";

                case UserFeatureAccess.MenuWithdraws:
                    return "Access to withdraws menu";

                case UserFeatureAccess.MenuSettings:
                    return "Access to settings menu";

                case UserFeatureAccess.CreatePayment:
                    return "Register new payment transaction";

                case UserFeatureAccess.MakeOkPayment:
                    return "Set ok notification to transaction";

                case UserFeatureAccess.BanClients:
                    return "Ban/unban clients";

                case UserFeatureAccess.MenuRegulators:
                    return "Access to Regulators menu";
                
                case UserFeatureAccess.SeoReports:
                    return "Access to SEO reports menu";

                case UserFeatureAccess.Offchain:
                    return "Offchain reports";
                
                case UserFeatureAccess.KycReports:
                    return "Access to KYC reports";

                case UserFeatureAccess.MenuKycPending:
                    return "KYC (waiting for review)";

                case UserFeatureAccess.DoNotShowSearchClient:
                    return "Do not show search client text-box";

                case UserFeatureAccess.ClientTools:
                    return "Access to client tools";

                case UserFeatureAccess.WithdrawLimits:
                    return "Withdraw limits";

                case UserFeatureAccess.ClientComments:
                    return "Client - add comment";

                case UserFeatureAccess.GoogleDriveManagement:
                    return "Google drive management";

                case UserFeatureAccess.MenuRegulation:
                    return "Regulation";
            }

            return userFeatureAccess.ToString();
        }


        public static string GetPaymentStatusColor(this PaymentStatus paymentStatus)
        {
            switch (paymentStatus)
            {
              case PaymentStatus.NotifyProcessed:
                    return "darkgreen";

                case PaymentStatus.NotifyDeclined:
                    return "darkred";

                case PaymentStatus.Processing:
                    return "yellow";
            }


            return "darkgray";

        }
    }
}
