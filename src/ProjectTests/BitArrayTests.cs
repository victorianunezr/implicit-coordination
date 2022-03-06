using System;
using ImplicitCoordination.DEL.utils;
using NUnit.Framework;

namespace DEL.Tests
{
    public class BitArrayTests
    {
        [Test]
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
