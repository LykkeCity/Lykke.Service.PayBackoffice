using Common.Cache;
using NUnit.Framework;

namespace CommonTests
{
    [TestFixture]
    public class MemoryCacheManagerTests
    {
        [Test]
        public void Can_set_and_get_object_from_cache()
        {
            var cacheManager = new MemoryCacheManager();
            cacheManager.Set("some_key_1", 3, int.MaxValue);

            Assert.AreEqual(3, cacheManager.Get<int>("some_key_1"));
        }

        [Test]
        public void Can_validate_whetherobject_is_cached()
        {
            var cacheManager = new MemoryCacheManager();
            cacheManager.Set("some_key_1", 3, int.MaxValue);
            cacheManager.Set("some_key_2", 4, int.MaxValue);

            Assert.AreEqual(true, cacheManager.IsSet("some_key_1"));
            Assert.AreEqual(false, cacheManager.IsSet("some_key_3"));
        }

        [Test]
        public void Can_clear_cache()
        {
            var cacheManager = new MemoryCacheManager();
            cacheManager.Set("some_key_1", 3, int.MaxValue);

            cacheManager.Clear();

            Assert.AreEqual(false, cacheManager.IsSet("some_key_1"));
        }
    }
}