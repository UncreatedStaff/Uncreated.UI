using Uncreated.Framework.UI;

namespace Uncreated.UI.Tests;

public class KeyPoolTests
{
    [Test]
    public void TestKeysDontRepeat()
    {
        HashSet<short> keys = new HashSet<short>();
        for (int i = 0; i < ushort.MaxValue; ++i)
        {
            short key = UnturnedUIKeyPool.Claim();

            Assert.That(keys.Add(key), Is.True, "Duplicate key: {0}.", key);
        }
    }

    [Test]
    public void TestNegativeOneIsntIncluded()
    {
        HashSet<short> keys = new HashSet<short>();
        for (int i = 0; i < ushort.MaxValue; ++i)
        {
            short key = UnturnedUIKeyPool.Claim();

            Assert.That(key, Is.Not.EqualTo((short)-1));
        }
    }
}