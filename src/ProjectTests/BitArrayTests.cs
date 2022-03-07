﻿using System;
using ImplicitCoordination.DEL;
using ImplicitCoordination.DEL.utils;
using NUnit.Framework;

namespace DEL.Tests
{
    public class BitArrayTests
    {
        [Test]
        public void GetAndSet_CorrectIndex()
        {
            var bb = new BitArray { data = 0b110 };

            Assert.IsTrue(bb.GetValue(1));
            Assert.IsFalse(bb.GetValue(0));
            Assert.IsTrue(bb.GetValue(2));

            bb.SetValue(4, true);
            Assert.IsTrue(bb.GetValue(4));

            bb.SetValue(4, false);
            Assert.IsFalse(bb.GetValue(4));
        }

        [Test]
        public void GetAndSet_OutOfRangeIndex()
        {
            var bb = new BitArray { data = UInt32.MaxValue };

            Assert.Throws<PropositionIdxOutOfRangeException>(() => bb.GetValue(32));
            Assert.Throws<PropositionIdxOutOfRangeException>(() => bb.GetValue(33));
            Assert.Throws<PropositionIdxOutOfRangeException>(() => bb.GetValue(0));
        }
    }
}
