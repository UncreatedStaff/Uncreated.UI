using System.Threading;

namespace Uncreated.Framework.UI;
public static class UnturnedUIKeyPool
{
    private static int _index;
    public static short Claim()
    {
        int rtn = Interlocked.Increment(ref _index);
        rtn %= ushort.MaxValue;
        rtn -= short.MaxValue;
        if (rtn >= -1)
            ++rtn;
        return (short)rtn;
    }
}