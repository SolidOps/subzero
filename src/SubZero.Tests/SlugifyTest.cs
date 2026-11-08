using SolidOps.SubZero;

namespace SubZero.Tests
{
    [TestClass]
    public class SlugifyTest
    {
        [TestMethod]
        public void TestThatSlugifyWorks()
        {
            Assert.AreEqual("test", TextHelper.GenerateSlug("Test"));
            Assert.AreEqual("test-other", TextHelper.GenerateSlug("TestOther"));
            Assert.AreEqual("test", TextHelper.GenerateSlug("TEST"));
            Assert.AreEqual("fa5", TextHelper.GenerateSlug("FA5"));
            Assert.AreEqual("this-is-a-test", TextHelper.GenerateSlug("ThisIsATest"));
        }
    }
}