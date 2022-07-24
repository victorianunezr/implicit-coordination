using System;
using System.Collections.Generic;
using ImplicitCoordination.DEL;
using ImplicitCoordination.utils;
using NUnit.Framework;

namespace DEL.Utils.Tests
{
    public class HashSetExtensionTests
    {
        [Test]
        public void ContainsSameWorlds()
        {
            // Arrange
            World w = new World(1);
            World u = new World(2);
            World v = new World(3);

            World q = new World(1);
            World s = new World(2);
            World t = new World(3);

            HashSet<World> set1 = new HashSet<World> { w, u, v };
            HashSet<World> set2 = new HashSet<World> { q, s, t };
            HashSet<World> set3 = new HashSet<World> { w, s, v };
            HashSet<World> set4 = new HashSet<World> { q, t };

            // Assert
            Assert.IsTrue(set1.ContainsSameWorlds(set2));
            Assert.IsTrue(set1.ContainsSameWorlds(set3));
            Assert.IsTrue(set2.ContainsSameWorlds(set3));
            Assert.IsFalse(set1.ContainsSameWorlds(set4));
        }
    }
}
