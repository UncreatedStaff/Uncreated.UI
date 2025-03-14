using System.ComponentModel;
using System.Threading;

namespace Uncreated.Framework.UI;

/// <summary>
/// Cycle through all valid <see cref="short"/> keys, skipping -1.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public static class UnturnedUIKeyPool
{
    private static int _index;

    /// <summary>
    /// Get the next key, skipping -1.
    /// </summary>
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