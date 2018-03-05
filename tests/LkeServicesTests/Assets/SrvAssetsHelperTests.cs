using System.Threading.Tasks;
using Autofac;
using Core.Bitcoin;
using Core.BitCoin;
using LkeServices.Assets.AssetGroups;
using LkeServices.Bitcoin;
using LkeServicesTests.Mocks;
using Moq;
using NUnit.Framework;
using Lykke.Service.Assets.Client.Models;
using System.Linq;
using Lykke.Service.Assets.Client;
using Core.Exchange;

namespace LkeServicesTests.Assets
{
    [TestFixture]
    public class SrvAssetsHelperTests
    {
        private SrvAssetsHelper _srvAssetsHelper;

        [SetUp]
        public void SetUp()
        {
            _srvAssetsHelper = new SrvAssetsHelper(null, null, null, null, null);
        }

        [Test]
        public void OrderAssets_orders_by_defaultOrder_property()
        {
            //arrange
            var assets = new[]{
                new Asset{ DisplayId = "CHF", DefaultOrder = 2 },
                new Asset{ DisplayId = "USD", DefaultOrder = 1 },
                new Asset{ DisplayId = "GEL", DefaultOrder = 3 },
            };

            //act
            var res = _srvAssetsHelper.OrderAssets(assets);

            //assert
            Assert.AreEqual(1, res.First().DefaultOrder);
            Assert.AreEqual(3, res.Last().DefaultOrder);
        }

        [Test]
        public void OrderAssets_orders_by_defaultOrder_and_then_by_displayId()
        {
            //arrange
            var assets = new[]{
                new Asset{ DisplayId = "USD", DefaultOrder = 1 },
                new Asset{ DisplayId = "CHF", DefaultOrder = 1 },
                new Asset{ DisplayId = "GEL", DefaultOrder = 2 },
            };

            //act
            var res = _srvAssetsHelper.OrderAssets(assets);

            //assert
            Assert.AreEqual("CHF", res.First().DisplayId);
            Assert.AreEqual("GEL", res.Last().DisplayId);
        }

        [Test]
        public void OrderAssets_orders_by_assetId_if_displayId_is_null()
        {
            //arrange
            var assets = new[]{
                new Asset{ DisplayId = "USD", DefaultOrder = 1 },
                new Asset{ Id = "CHF", DefaultOrder = 1 },
                new Asset{ DisplayId = "GEL", DefaultOrder = 2 },
            };

            //act
            var res = _srvAssetsHelper.OrderAssets(assets);

            //assert
            Assert.AreEqual("CHF", res.First().Id);
            Assert.AreEqual("GEL", res.Last().DisplayId);
        }
    }
}
