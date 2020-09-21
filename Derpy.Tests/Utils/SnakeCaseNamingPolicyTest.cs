using Derpy.Utils.Tumblr;
using Xunit;

namespace Derpy.Tests.Utils
{
    public class SnakeCaseNamingPolicyTest
    {
        private readonly SnakeCaseNamingPolicy _namingPolicy;

        public SnakeCaseNamingPolicyTest() => _namingPolicy = new SnakeCaseNamingPolicy();

        [Fact]
        public void Test_Standard()
        {
            var t = _namingPolicy.ConvertName("TestName");
            Assert.Equal("test_name", t);
        }

        [Fact]
        public void Test_Single()
        {
            var t = _namingPolicy.ConvertName("Test");
            Assert.Equal("test", t);
        }

        [Fact]
        public void Test_Multiple()
        {
            var t = _namingPolicy.ConvertName("TestLongerName");
            Assert.Equal("test_longer_name", t);
        }

        [Fact]
        public void Test_Consecutive()
        {
            var t = _namingPolicy.ConvertName("TestXMLParsing");
            Assert.Equal("test_x_m_l_parsing", t);
        }
    }
}
