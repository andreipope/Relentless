using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Loom.ZombieBattleground.Test
{
    public class ListTests
    {
        [Test]
        public void UniqueList()
        {
            UniqueList<int> uniqueList = new UniqueList<int>(new List<int>(new[] { 1, 2, 3 }));

            Assert.DoesNotThrow(() => uniqueList.Add(4));
            Assert.AreEqual(new[] { 1, 2, 3, 4 }, uniqueList.ToArray());
            Assert.Throws<ArgumentException>(() => uniqueList.Add(1), "Item \"1\" is already in the list");

            Assert.Throws<ArgumentException>(
                () => new UniqueList<int>(new List<int>(new[] { 1, 2, 3, 3 })),
                "Source list contained duplicate value \"3\"");
        }
    }
}
