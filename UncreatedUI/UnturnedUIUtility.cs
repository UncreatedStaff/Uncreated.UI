using System;
using System.Globalization;

namespace Uncreated.Framework.UI;

/// <summary>
/// Utility methods for working with UIs.
/// </summary>
public static class UnturnedUIUtility
{
    /// <summary>
    /// Quickly replace a {n} format placeholder with a value. No special formatting is done unlike <see cref="string.Format(string,object)"/>.
    /// </summary>
    /// <param name="input">The text to be formatted.</param>
    /// <param name="value">The value to insert in the placeholder, can be empty.</param>
    /// <param name="index">The number, 'n', to look for.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.</exception>
    public static unsafe string QuickFormat(ReadOnlySpan<char> input, ReadOnlySpan<char> value, int index)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        Span<char> searchTarget = stackalloc char[2 + CountDigits(index)];
        searchTarget[0] = '{';
        index.TryFormat(searchTarget[1..], out int charsWritten, "D0", CultureInfo.InvariantCulture);
        searchTarget[charsWritten + 1] = '}';

        int dLen = value.Length - searchTarget.Length;

        int newLen = input.Length;
        int placeInd = -searchTarget.Length;
        while (true)
        {
            int stInd = placeInd + searchTarget.Length;
            placeInd = input.Slice(stInd).IndexOf(searchTarget, StringComparison.Ordinal);
            if (placeInd < 0)
                break;

            placeInd += stInd;
            newLen += dLen;
        }

        fixed (char* ptr = input)
        fixed (char* srh = searchTarget)
        fixed (char* vlp = value)
        {
            QuickFmtStringState state = default;
            state.Input = ptr;
            state.InputLen = input.Length;
            state.Search = srh;
            state.SearchLen = searchTarget.Length;
            state.Value = vlp;
            state.ValueLen = value.Length;
            return string.Create(newLen, state, static (span, state) =>
            {
                Span<char> searchTarget = new Span<char>(state.Search, state.SearchLen);
                ReadOnlySpan<char> input = new ReadOnlySpan<char>(state.Input, state.InputLen);
                ReadOnlySpan<char> value = new ReadOnlySpan<char>(state.Value, state.ValueLen);
                int sLen = searchTarget.Length, iLen = input.Length, vLen = value.Length;
                int placeInd = -state.SearchLen;
                int index = 0;
                while (true)
                {
                    int lastIndex = placeInd + sLen;
                    if (lastIndex > iLen)
                        break;
                    placeInd = input[lastIndex..].IndexOf(searchTarget, StringComparison.Ordinal);
                    if (placeInd == -1)
                    {
                        if (lastIndex != iLen)
                            input.Slice(lastIndex, iLen - lastIndex).CopyTo(span[index..]);
                            
                        break;
                    }

                    int ct = placeInd;
                    placeInd += lastIndex;
                    if (ct != 0)
                    {
                        input.Slice(lastIndex, ct).CopyTo(span.Slice(index, ct));
                        index += ct;
                    }

                    if (vLen == 0)
                        continue;

                    value.CopyTo(span.Slice(index, vLen));
                    index += vLen;
                }
            });
        }
    }
    private unsafe struct QuickFmtStringState
    {
        public char* Input;
        public char* Search;
        public char* Value;
        public int InputLen;
        public int SearchLen;
        public int ValueLen;
    }
    private static int CountDigits(int num)
    {
        int c = (uint)Math.Abs(num) switch
        {
            <= 9 => 1,
            <= 99 => 2,
            <= 999 => 3,
            <= 9999 => 4,
            <= 99999 => 5,
            <= 999999 => 6,
            <= 9999999 => 7,
            <= 99999999 => 8,
            <= 999999999 => 9,
            _ => 10
        };
        return c;
    }

}