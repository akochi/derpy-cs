using Xunit;

namespace Derpy.Tests
{
    public class DrawalongTest
    {
        private Drawalong _drawalong = new Drawalong();

        [Fact]
        public void Test_NotActive()
        {
            Assert.False(_drawalong.Active);
        }
    }
}
