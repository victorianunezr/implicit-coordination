using ImplicitCoodrination.DEL;
using ImplicitCoordination.DEL.utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImplicitCoordination.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            //World w = new World(new BitVector32(0b110));

            //// Assert
            //Assert.IsTrue(w.valuation[1]);
            //Assert.IsFalse(w.valuation[0]);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void BitArrayTest()
        {
            var bb = new BitArray { data = 6 };

            Assert.IsTrue(bb.GetValue(1));
            Assert.IsFalse(bb.GetValue(0));
            Assert.IsTrue(bb.GetValue(2));

            bb.SetValue(4, true);
            Assert.IsTrue(bb.GetValue(4));

            bb.SetValue(4, false);
            Assert.IsFalse(bb.GetValue(4));
        }
    }
}
