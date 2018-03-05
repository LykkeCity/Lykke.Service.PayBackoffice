using NUnit.Framework;

namespace BackOfficeTests.Framework
{
    public static class AssertExtensions
    {
        public static void ShouldBeEqualTo(this object actual, object expected)
        {
            Assert.AreEqual(expected, actual);
        }
    }
}
