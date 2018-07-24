using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Configuration;
using BackOffice.Areas.LykkePay.Models;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;

namespace BackOffice.Binders
{
    public class MapperProvider
    {
        public IMapper GetMapper()
        {
            var mce = new MapperConfigurationExpression();

            CreatePaymentRequestsMaps(mce);

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
    }
}
