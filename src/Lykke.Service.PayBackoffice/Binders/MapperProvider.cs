using AutoMapper;
using AutoMapper.Configuration;
using BackOffice.Areas.LykkePay.Models;
using Core.Staff;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;

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
            mce.CreateMap<AddStaffDialogViewModel, NewStaffCommand>(MemberList.Destination)
                .ForMember(dest => dest.MerchantId, opt => opt.MapFrom(src => src.SelectedMerchant));

            mce.CreateMap<AddStaffDialogViewModel, UpdateStaffCommand>(MemberList.Destination)
                .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.Id));
        }
    }
}
