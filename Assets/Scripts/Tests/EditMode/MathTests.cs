using Loom.ZombieBattleground.Helpers;
using NUnit.Framework;

namespace Loom.ZombieBattleground.Test
{
    [Category("EditQuickSubset")]
    public class MathTests
    {
        [Test]
        public void RepeatTest()
        {
            Assert.AreEqual(0, MathUtility.Repeat(0, 5));
            Assert.AreEqual(4, MathUtility.Repeat(-1, 5));
            Assert.AreEqual(4, MathUtility.Repeat(-1 - 5 - 5, 5));
            Assert.AreEqual(1, MathUtility.Repeat(6, 5));
            Assert.AreEqual(1, MathUtility.Repeat(6 + 5, 5));

            Assert.AreEqual(0, MathUtility.Repeat(0, 1));
            Assert.AreEqual(0, MathUtility.Repeat(1, 1));
            Assert.AreEqual(0, MathUtility.Repeat(2, 1));
        }
    }
}
