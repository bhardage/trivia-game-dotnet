using Microsoft.VisualStudio.TestTools.UnitTesting;
using TriviaGame.Utils;

namespace TriviaGameTests.Utils
{
    [TestClass]
    public class SlackUtilsTest
    {
        [TestMethod]
        public void TestNormalizeIdWithNullSlackId()
        {
            Assert.IsNull(SlackUtils.NormalizeId(null));
        }

        [TestMethod]
        public void TestNormalizeIdWithInvalidSlackId()
        {
            Assert.AreEqual("garbage", SlackUtils.NormalizeId("garbage"));
        }

        [TestMethod]
        public void TestNormalizeIdWithValidSlackId()
        {
            Assert.AreEqual("U12345", SlackUtils.NormalizeId("<@U12345>"));
        }

        [TestMethod]
        public void TestNormalizeIdWithValidSlackIdAndUsername()
        {
            Assert.AreEqual("U12345", SlackUtils.NormalizeId("<@U12345|jsmith>"));
        }

        [TestMethod]
        public void TestNormalizeIdWithValidSlackIdAndEmptyUsername()
        {
            Assert.AreEqual("U12345", SlackUtils.NormalizeId("<@U12345|>"));
        }
    }
}
