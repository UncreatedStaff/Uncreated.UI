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
    /// <param name="value">The positive number to insert in the placeholder.</param>
    /// <param name="index">The number, 'n', to look for.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.</exception>
    public static unsafe string QuickFormat(ReadOnlySpan<char> input, int value, int index)
    {
        if (input.Length == 0)
            return string.Empty;

        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value));

        Span<char> valueStr = stackalloc char[CountDigits(value)];
        value.TryFormat(valueStr, out _, "D0", CultureInfo.InvariantCulture);

        return QuickFormat(input, valueStr, index);
    }

    /// <summary>
    /// Get the length from optional 'to' or 'length' parameters.
    /// </summary>
    internal static int ResolveLength(int start, int length, int to)
    {
        if (to > -1)
        {
            int newLen = to - start + 1;

            if (length > -1 && length != newLen)
                throw new ArgumentException(nameof(to), "Inconsistent values in arguments: 'to', and 'length'.");

            return newLen;
        }

        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Must either specify 'to' or 'length'.");

        return length;
    }

    /// <summary>
    /// Quickly replace a {n} format placeholder with a value. No special formatting is done unlike <see cref="string.Format(string,object)"/>.
    /// </summary>
    /// <param name="input">The text to be formatted.</param>
    /// <param name="value">The value to insert in the placeholder, can be empty.</param>
    /// <param name="index">The number, 'n', to look for.</param>
    /// <param name="cleanJoin">Join these characters if they're left sequentially when <paramref name="value"/> is an empty string.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.</exception>
    public static unsafe string QuickFormat(ReadOnlySpan<char> input, ReadOnlySpan<char> value, int index, char? cleanJoin = null)
    {
        if (input.Length == 0)
            return string.Empty;

        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        bool hasCleanJoin = value.Length == 0 && cleanJoin.HasValue;
        char cleanJoinValue = hasCleanJoin ? cleanJoin!.Value : default;

        Span<char> searchTarget = stackalloc char[2 + CountDigits(index)];
        searchTarget[0] = '{';
        index.TryFormat(searchTarget[1..], out int charsWritten, "D0", CultureInfo.InvariantCulture);
        searchTarget[charsWritten + 1] = '}';

        int searchTargetLength = searchTarget.Length;
        int dLen = value.Length - searchTargetLength;

        int newLen = input.Length;
        int placeInd = -searchTargetLength;
        bool any = false;
        while (true)
        {
            int stInd = placeInd + searchTargetLength;
            ReadOnlySpan<char> slice = input.Slice(stInd);
            placeInd = slice.IndexOf(searchTarget, StringComparison.Ordinal);
            if (placeInd < 0)
                break;

            if (hasCleanJoin && placeInd != 0 && placeInd != slice.Length - searchTargetLength)
            {
                if (slice[placeInd - 1] == cleanJoinValue && slice[placeInd + searchTargetLength] == cleanJoinValue)
                    --newLen;
            }

            any = true;
            placeInd += stInd;
            newLen += dLen;
        }

        if (!any)
        {
            return new string(input);
        }

        fixed (char* ptr = input)
        fixed (char* srh = searchTarget)
        fixed (char* vlp = value)
        {
            QuickFmtStringState state = default;
            state.Input = ptr;
            state.InputLen = input.Length;
            state.Search = srh;
            state.SearchLen = searchTargetLength;
            state.Value = vlp;
            state.ValueLen = value.Length;
            state.HasCleanJoin = hasCleanJoin;
            state.CleanJoinValue = cleanJoinValue;
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
                    ReadOnlySpan<char> slice = input[lastIndex..];
                    placeInd = slice.IndexOf(searchTarget, StringComparison.Ordinal);
                    if (placeInd == -1)
                    {
                        if (lastIndex != iLen)
                            input.Slice(lastIndex, iLen - lastIndex).CopyTo(span[index..]);

                        break;
                    }

                    bool cleanJoin = state.HasCleanJoin
                                     && placeInd != 0
                                     && placeInd != slice.Length - sLen
                                     && slice[placeInd - 1] == state.CleanJoinValue
                                     && slice[placeInd + sLen] == state.CleanJoinValue;

                    int ct = placeInd;
                    placeInd += lastIndex;
                    if (ct != 0)
                    {
                        if (cleanJoin)
                            --ct;
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
        public bool HasCleanJoin;
        public char CleanJoinValue;
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

    /// <summary>
    /// Parse the last value of a path to get the actual element name.
    /// </summary>
    public static ReadOnlyMemory<char> GetNameFromPathOrName(ReadOnlyMemory<char> path)
    {
        switch (path.Length)
        {
            case 0:
                return default;
        }

        int ind = path.Span.LastIndexOf('/');

        if (ind == -1)
            return path;

        return ind == path.Length - 1
            ? default
            : path.Slice(ind + 1, path.Length - ind - 1);
    }

    /// <summary>
    /// Parse everything but the last value of a path to get the path to the element, including the trailing slash but removing the leading slash if there is one.
    /// </summary>
    public static ReadOnlyMemory<char> GetPathFromPathOrName(ReadOnlyMemory<char> path)
    {
        switch (path.Length)
        {
            case 0:
                return default;
        }

        ReadOnlySpan<char> span = path.Span;

        int startIndex = 0;
        while (startIndex < span.Length && span[startIndex] == '/')
            ++startIndex;

        int ind = span.LastIndexOf('/') + 1;
        return ind == 0 ? default : path.Slice(startIndex, ind - startIndex);
    }
}