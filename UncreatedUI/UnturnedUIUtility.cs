using System;
using System.ComponentModel;
using System.Globalization;

namespace Uncreated.Framework.UI;

/// <summary>
/// Utility methods for working with UIs.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Advanced)]
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
    /// Check if a path is rooted instead of being relative.
    /// </summary>
    public static bool IsRooted(ReadOnlySpan<char> path)
    {
        path = path.Trim();

        if (path.IsEmpty) return true;

        if (path[0] == '.')
        {
            if (path.Length == 1 || path[1] == '/')
                return false;

            if (path[1] == '.' && (path.Length == 2 || path[2] == '/'))
                return false;
        }

        return true;
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
                throw new ArgumentException(nameof(to), Properties.Resources.Exception_InconsistantLengthArguments);

            return newLen;
        }

        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), Properties.Resources.Exception_MissingLengthArguments);

        return length;
    }

    /// <summary>
    /// Combine a path and element name (or another path).
    /// </summary>
    public static unsafe string CombinePath(ReadOnlySpan<char> path, ReadOnlySpan<char> name)
    {
        path = TrimSlashes(path, true, true);
        name = TrimSlashes(name, true, true);

        if (path.Length == 0)
            return name.Length == 0 ? string.Empty : new string(name);

        int len = path.Length + name.Length + 1;
        CombinePathState state = default;
        fixed (char* pathPtr = path)
        fixed (char* namePtr = name)
        {
            state.PathPtr = pathPtr;
            state.PathLen = path.Length;
            state.NamePtr = namePtr;
            state.NameLen = name.Length;
            return string.Create(len, state, (span, state) =>
            {
                ReadOnlySpan<char> path = new ReadOnlySpan<char>(state.PathPtr, state.PathLen);
                ReadOnlySpan<char> name = new ReadOnlySpan<char>(state.NamePtr, state.NameLen);
                path.CopyTo(span);
                span[state.PathLen] = '/';
                name.CopyTo(span.Slice(state.PathLen + 1));
            });
        }
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

            if (hasCleanJoin && placeInd <= slice.Length - searchTargetLength)
            {
                if ((placeInd == 0 || slice[placeInd - 1] == cleanJoinValue) && (placeInd == slice.Length - searchTargetLength || slice[placeInd + searchTargetLength] == cleanJoinValue) && slice.Length != searchTargetLength)
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
                                     && placeInd <= slice.Length - sLen
                                     && (placeInd == 0 || slice[placeInd - 1] == state.CleanJoinValue)
                                     && (placeInd == slice.Length - sLen || slice[placeInd + sLen] == state.CleanJoinValue)
                                     && slice.Length != sLen;

                    int ct = placeInd;
                    placeInd += lastIndex;
                    if (ct != 0)
                    {
                        if (cleanJoin)
                        {
                            --ct;
                        }
                        input.Slice(lastIndex, ct).CopyTo(span.Slice(index, ct));
                        index += ct;
                    }
                    else if (cleanJoin)
                        ++placeInd;

                    if (vLen == 0)
                        continue;

                    value.CopyTo(span.Slice(index, vLen));
                    index += vLen;
                }
            });
        }
    }

    /// <summary>
    /// Parse the last value of a path to get the actual element name.
    /// </summary>
    public static ReadOnlyMemory<char> GetNameFromPathOrName(ReadOnlyMemory<char> path)
    {
        if (path.Length == 0)
            return default;

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
        if (path.Length == 0)
            return default;

        ReadOnlySpan<char> span = path.Span;

        int startIndex = 0;
        while (startIndex < span.Length && span[startIndex] == '/')
            ++startIndex;

        int ind = span.LastIndexOf('/') + 1;
        return ind == 0 ? default : path.Slice(startIndex, ind - startIndex - (ind == path.Length - startIndex ? 1 : 0));
    }

    /// <summary>
    /// Find the full path of <paramref name="path"/> if it is relative to <paramref name="relativeTo"/>. <paramref name="path"/> must start with './' or '../' unless <paramref name="assumeRelative"/> is <see langword="true"/>.
    /// </summary>
    /// <exception cref="ArgumentException">Invalid paths.</exception>
    public static unsafe string ResolveRelativePath(ReadOnlySpan<char> relativeTo, ReadOnlySpan<char> path, bool assumeRelative = false)
    {
        if (path.Length == 0)
            return assumeRelative ? new string(relativeTo) : string.Empty;

        if (path.Length >= 2)
        {
            if (path[0] == '\\' && char.IsWhiteSpace(path[1]))
                path = path[1..];
        }
        else path = path.TrimStart();


        if (path[0] != '.' && !assumeRelative)
        {
            if (path[0] == '~')
                path = path[1..];

            path = TrimSlashes(path, true, false);
            return new string(path);
        }
        if (path[0] == '~')
        {
            path = TrimSlashes(path[1..], true, false);
            return new string(path);
        }

        int pathSectionCt = CountPathSplits(path);
        Range* pathSections = stackalloc Range[pathSectionCt];
        SplitPath(new Span<Range>(pathSections, pathSectionCt), path);

        int relativeToSectionCt = CountPathSplits(relativeTo);
        Range* relativeToSections = stackalloc Range[relativeToSectionCt];
        SplitPath(new Span<Range>(relativeToSections, relativeToSectionCt), relativeTo);

        int totalSize = 0;
        for (int i = 0; i < relativeToSectionCt; ++i)
        {
            ReadOnlySpan<char> segment = relativeTo[relativeToSections[i]];

            if (segment.Length == 1 && segment[0] == '.' || segment.Length == 2 && segment[0] == '.' && segment[1] == '.')
                throw new ArgumentException(Properties.Resources.Exception_NavigationPathsNotSupportedInRelevantPath, nameof(relativeTo));

            totalSize += segment.Length;
        }

        int minLevel = 0;
        int level = 0;
        int actualPathElements = relativeToSectionCt;
        int pathElementsToUseFromRelativeTo = relativeToSectionCt;
        for (int i = 0; i < pathSectionCt; ++i)
        {
            ReadOnlySpan<char> segment = path[pathSections[i]];

            if (!assumeRelative && i == 0 && (segment.Length > 2 || segment[0] != '.' || (segment.Length == 2 && segment[1] != '.')))
            {
                path = TrimSlashes(path, true, false);
                return new string(path);
            }

            if (segment.Length == 2 && segment[0] == '.' && segment[1] == '.')
            {
                --level;
            }
            else if (segment.Length != 1 || segment[0] != '.')
            {
                if (level < 0 && -level > relativeToSectionCt)
                {
                    int ind = relativeToSectionCt - -level + 1;
                    if (ind >= relativeToSectionCt || !segment.Equals(relativeTo[relativeToSections[ind]], StringComparison.Ordinal))
                        throw new ArgumentException(Properties.Resources.Exception_RelativePathExceedsRoot, nameof(path));
                }
                ++level;
                minLevel = level;
                for (int j = 0; j <= -level; ++j)
                {
                    totalSize -= GetRangeLength(relativeToSections[relativeToSectionCt - j - 1], relativeTo.Length);
                    --actualPathElements;
                    --pathElementsToUseFromRelativeTo;
                }

                level = 0;

                ++actualPathElements;
                totalSize += segment.Length;
            }
            else
            {
                continue;
            }

            if (minLevel > level)
                minLevel = level;
        }

        for (int j = 0; j < -level; ++j)
        {
            totalSize -= GetRangeLength(relativeToSections[relativeToSectionCt - j - 1], relativeTo.Length);
            --actualPathElements;
            --pathElementsToUseFromRelativeTo;
        }

        int numToSkip = 0;
        for (int i = pathSectionCt - 1; i >= relativeToSectionCt - pathElementsToUseFromRelativeTo; --i)
        {
            ReadOnlySpan<char> segment = path[pathSections[i]];
            if (numToSkip > 0)
            {
                --actualPathElements;
                totalSize -= segment.Length;
                --numToSkip;
                continue;
            }

            if (segment.Length == 2 && segment[0] == '.' && segment[1] == '.')
            {
                ++numToSkip;
            }
        }

        if (actualPathElements == 0)
            return string.Empty;

        if (minLevel < 0 && -minLevel > relativeToSectionCt)
            throw new ArgumentException(Properties.Resources.Exception_RelativePathExceedsRoot, nameof(path));

        totalSize += actualPathElements - 1;

        RelativePathState state = default;
        state.PathSegments = pathSections;
        state.RelativeToSegments = relativeToSections;
        state.PathSegmentCount = pathSectionCt;
        state.OutputSegments = actualPathElements;
        state.PathLength = path.Length;
        state.RelativeToLength = relativeTo.Length;
        state.PathElementsToUseFromRelativeTo = pathElementsToUseFromRelativeTo;

        fixed (char* pathPtr = path)
        fixed (char* relativeToPtr = relativeTo)
        {
            state.Path = pathPtr;
            state.RelativeTo = relativeToPtr;

            return string.Create(totalSize, state, (span, state) =>
            {
                ReadOnlySpan<char> path = new ReadOnlySpan<char>(state.Path, state.PathLength);
                ReadOnlySpan<char> relativeTo = new ReadOnlySpan<char>(state.RelativeTo, state.RelativeToLength);

                int index = 0;
                for (int i = 0; i < state.PathElementsToUseFromRelativeTo; ++i)
                {
                    ReadOnlySpan<char> segment = relativeTo[state.RelativeToSegments[i]];
                    segment.CopyTo(span.Slice(index));
                    index += segment.Length;
                    if (i != state.OutputSegments - 1)
                    {
                        span[index] = '/';
                        ++index;
                    }
                }

                state.OutputSegments -= state.PathElementsToUseFromRelativeTo;

                int numToSkip = 0;
                int bytesCopied = 0;
                int ct = 0;
                for (int i = state.PathSegmentCount - 1; i >= 0; --i)
                {
                    if (numToSkip > 0)
                    {
                        --numToSkip;
                        continue;
                    }

                    ReadOnlySpan<char> segment = path[state.PathSegments[i]];
                    if (segment.Length == 2 && segment[0] == '.' && segment[1] == '.')
                    {
                        ++numToSkip;
                        continue;
                    }

                    if (segment.Length == 1 && segment[0] == '.')
                        continue;

                    segment.CopyTo(span.Slice(span.Length - bytesCopied - segment.Length));
                    bytesCopied += segment.Length;
                    ++ct;

                    if (ct == state.OutputSegments)
                        continue;

                    span[span.Length - bytesCopied - 1] = '/';
                    ++bytesCopied;
                }
            });
        }
    }
    internal static string GetPresetValue(string parentName, string? inputValue, string defaultSuffix)
    {
        if (inputValue == null)
        {
            return parentName + defaultSuffix;
        }

        return ResolveRelativePath(parentName, inputValue);
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
    private static int GetRangeLength(Range r, int collectionLength)
    {
        int offset1 = r.Start.GetOffset(collectionLength);
        int offset2 = r.End.GetOffset(collectionLength);
        return offset2 - offset1;
    }
    private static int CountPathSplits(ReadOnlySpan<char> path)
    {
        if (path.Length == 0)
        {
            return 0;
        }

        int sectionCt = 1;
        int lastSlashInd = -1;
        while (true)
        {
            int stInd = lastSlashInd + 1;
            lastSlashInd = path.Slice(stInd).IndexOf('/');

            if (lastSlashInd == -1 || lastSlashInd == path.Length - stInd - 1)
                break;

            lastSlashInd += stInd;
            if (lastSlashInd == stInd)
                continue;

            if (lastSlashInd - stInd == 0)
                continue;

            ++sectionCt;
        }

        return sectionCt;
    }
    private static void SplitPath(Span<Range> splitOut, ReadOnlySpan<char> path)
    {
        int sectionCt = splitOut.Length;
        if (sectionCt == 0)
            return;
        if (sectionCt == 1)
        {
            TrimCharacters(path, 0, path.Length, out int newOffset, out int newCount, true, true, '/');
            splitOut[0] = new Range(new Index(newOffset), new Index(newOffset + newCount));
        }
        else
        {
            int lastSlashInd = -1;
            int index = -1;
            while (lastSlashInd < path.Length - 1)
            {
                int stInd = lastSlashInd + 1;
                ReadOnlySpan<char> span = path.Slice(stInd);
                lastSlashInd = span.IndexOf('/');

                if (lastSlashInd + stInd == stInd && lastSlashInd != -1)
                {
                    lastSlashInd += stInd;
                    continue;
                }

                int endIndex = lastSlashInd != -1 ? lastSlashInd + stInd : path.Length;

                if (endIndex - stInd != 0)
                {
                    splitOut[++index] = new Range(new Index(stInd), new Index(endIndex));
                }

                if (lastSlashInd == -1)
                    break;

                lastSlashInd += stInd;
            }
        }
    }
    internal static ReadOnlySpan<char> TrimSlashes(ReadOnlySpan<char> span, bool front, bool end)
    {
        TrimCharacters(span, 0, span.Length, out int newOffset, out int newCount, front, end, '/');
        return span.Slice(newOffset, newCount);
    }
    private static void TrimCharacters(ReadOnlySpan<char> span, int offset, int count, out int newOffset, out int newCount, bool front, bool end, char character)
    {
        newOffset = offset;
        newCount = count;

        int startInd = offset;
        int endInd = offset + count - 1;
        if (endInd < offset)
            return;

        if (startInd == endInd)
        {
            if (span[startInd] != character)
                return;

            newOffset = 0;
            newCount = 0;
            return;
        }

        if (front)
        {
            while (startInd < span.Length && span[startInd] == character)
                ++startInd;
        }
        if (end)
        {
            while (endInd >= 0 && span[endInd] == character)
                --endInd;
        }

        if (endInd < 0 || startInd >= span.Length)
        {
            newOffset = 0;
            newCount = 0;
        }

        newOffset = startInd;
        newCount = endInd - startInd + 1;
    }

    private unsafe struct RelativePathState
    {
        public Range* PathSegments;
        public Range* RelativeToSegments;
        public char* Path;
        public char* RelativeTo;
        public int PathLength;
        public int RelativeToLength;
        public int PathSegmentCount;
        public int OutputSegments;
        public int PathElementsToUseFromRelativeTo;
    }
    private unsafe struct CombinePathState
    {
        public char* PathPtr;
        public char* NamePtr;
        public int PathLen;
        public int NameLen;
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
}