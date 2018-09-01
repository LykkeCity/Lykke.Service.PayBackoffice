using AutoMapper;
using AutoMapper.Configuration;
using BackOffice.Areas.LykkePay.Models;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Lykke.Service.PayInvoice.Contract.Commands;

namespace BackOffice.Binders
{
    public class MapperProvider
    {
        public IMapper GetMapper()
        {
            var mce = new MapperConfigurationExpression();

            CreatePaymentRequestsMaps(mce);

            CreateStaffMaps(mce);

            var mc = new MapperConfiguration(mce);
            mc.AssertConfigurationIsValid();

            return new Mapper(mc);
        }

        private void CreatePaymentRequestsMaps(MapperConfigurationExpression mce)
        {
            mce.CreateMap<PaymentRequestModel, PaymentRequestViewModel>()
                .ForMember(o => o.PaymentAssetGeneralSettings, e => e.Ignore())
                .ForMember(o => o.SettlementAssetGeneralSettings, e => e.Ignore());
        }

        private void CreateStaffMaps(MapperConfigurationExpression mce)
        {
            mce.CreateMap<AddStaffDialogViewModel, RegisterEmployeeCommand>(MemberList.Destination)
                .ForMember(dest => dest.MerchantId, opt => opt.MapFrom(src => src.SelectedMerchant));

            mce.CreateMap<AddStaffDialogViewModel, UpdateEmployeeCommand>(MemberList.Destination)
                .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.MerchantId, opt => opt.MapFrom(src => src.SelectedMerchant))
                .ForMember(dest => dest.IsInternalSupervisor, opt => opt.UseValue(false));
        }
    }
}
