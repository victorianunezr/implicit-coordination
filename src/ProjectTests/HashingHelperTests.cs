using System;
using System.Collections.Generic;
using ImplicitCoordination.utils;
using NUnit.Framework;

namespace DELTests
{
    public class HashingHelperTests
    {

        [Test]
        public void SortListOfTuples()
        {
            // Arrange
            List<(ulong, ulong)> sortedList = new List<(ulong, ulong)>() { (1) };
            List<(ulong, ulong)> list = new List<(ulong, ulong)>(){ (4, 1), (2, 5), (4, 2), (2, 1), (2, 2) };

            // Act
            HashingHelper.SortListOfTuples(list);

            // Assert
            CollectionAssert.Equals()
            
        }
    }
}
