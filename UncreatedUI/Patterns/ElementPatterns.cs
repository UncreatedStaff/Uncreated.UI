using DanielWillett.ReflectionTools;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Uncreated.Framework.UI.Presets;

namespace Uncreated.Framework.UI.Patterns;

/// <summary>
/// Create arrays of elements based on a pattern.
/// </summary>
public static class ElementPatterns
{
    internal static readonly Dictionary<Type, PatternTypeInfo> TypeInfo = new Dictionary<Type, PatternTypeInfo>();

    /// <summary>
    /// Add <paramref name="handler"/> to all <see cref="UnturnedButton.OnClicked"/> events in <paramref name="buttons"/> after being transformed by a <paramref name="selector"/>.
    /// </summary>
    public static void SubscribeAll<T>(IEnumerable<T> buttons, Func<T, UnturnedButton> selector, ButtonClicked handler)
    {
        foreach (T button in buttons)
            selector(button).OnClicked += handler;
    }

    /// <summary>
    /// Remove <paramref name="handler"/> from all <see cref="UnturnedButton.OnClicked"/> events in <paramref name="buttons"/> after being transformed by a <paramref name="selector"/>.
    /// </summary>
    public static void UnsubscribeAll<T>(IEnumerable<T> buttons, Func<T, UnturnedButton> selector, ButtonClicked handler)
    {
        foreach (T button in buttons)
            selector(button).OnClicked -= handler;
    }

    /// <summary>
    /// Add <paramref name="handler"/> to all <see cref="UnturnedTextBox.OnTextUpdated"/> events in <paramref name="textBoxes"/> after being transformed by a <paramref name="selector"/>.
    /// </summary>
    public static void SubscribeAll<T>(IEnumerable<T> textBoxes, Func<T, UnturnedTextBox> selector, TextUpdated handler)
    {
        foreach (T textBox in textBoxes)
            selector(textBox).OnTextUpdated += handler;
    }

    /// <summary>
    /// Remove <paramref name="handler"/> from all <see cref="UnturnedTextBox.OnTextUpdated"/> events in <paramref name="textBoxes"/> after being transformed by a <paramref name="selector"/>.
    /// </summary>
    public static void UnsubscribeAll<T>(IEnumerable<T> textBoxes, Func<T, UnturnedTextBox> selector, TextUpdated handler)
    {
        foreach (T textBox in textBoxes)
            selector(textBox).OnTextUpdated -= handler;
    }

    /// <summary>
    /// Add <paramref name="handler"/> to all <see cref="IButton.OnClicked"/> events in <paramref name="buttons"/>.
    /// </summary>
    public static void SubscribeAll<T>(IEnumerable<T> buttons, ButtonClicked handler) where T : IButton
    {
        foreach (T button in buttons)
            button.OnClicked += handler;
    }

    /// <summary>
    /// Remove <paramref name="handler"/> from all <see cref="IButton.OnClicked"/> events in <paramref name="buttons"/>.
    /// </summary>
    public static void UnsubscribeAll<T>(IEnumerable<T> buttons, ButtonClicked handler) where T : IButton
    {
        foreach (T button in buttons)
            button.OnClicked -= handler;
    }

    /// <summary>
    /// Add <paramref name="handler"/> to all <see cref="ITextBox.OnTextUpdated"/> events in <paramref name="textBoxes"/>.
    /// </summary>
    public static void SubscribeAll<T>(IEnumerable<T> textBoxes, TextUpdated handler) where T : ITextBox
    {
        foreach (T textBox in textBoxes)
            textBox.OnTextUpdated += handler;
    }

    /// <summary>
    /// Remove <paramref name="handler"/> from all <see cref="ITextBox.OnTextUpdated"/> events in <paramref name="textBoxes"/>.
    /// </summary>
    public static void UnsubscribeAll<T>(IEnumerable<T> textBoxes, TextUpdated handler) where T : ITextBox
    {
        foreach (T textBox in textBoxes)
            textBox.OnTextUpdated -= handler;
    }

    /// <summary>
    /// Add <paramref name="handler"/> to all <see cref="IButton.OnClicked"/> events in <paramref name="buttons"/> after being transformed by a <paramref name="selector"/>.
    /// </summary>
    public static void SubscribeAll<T, TButton>(IEnumerable<T> buttons, Func<T, TButton> selector, ButtonClicked handler) where TButton : IButton
    {
        foreach (T button in buttons)
            selector(button).OnClicked += handler;
    }

    /// <summary>
    /// Remove <paramref name="handler"/> from all <see cref="IButton.OnClicked"/> events in <paramref name="buttons"/> after being transformed by a <paramref name="selector"/>.
    /// </summary>
    public static void UnsubscribeAll<T, TButton>(IEnumerable<T> buttons, Func<T, TButton> selector, ButtonClicked handler) where TButton : IButton
    {
        foreach (T button in buttons)
            selector(button).OnClicked -= handler;
    }

    /// <summary>
    /// Add <paramref name="handler"/> to all <see cref="ITextBox.OnTextUpdated"/> events in <paramref name="textBoxes"/> after being transformed by a <paramref name="selector"/>.
    /// </summary>
    public static void SubscribeAll<T, TTextBox>(IEnumerable<T> textBoxes, Func<T, TTextBox> selector, TextUpdated handler) where TTextBox : ITextBox
    {
        foreach (T textBox in textBoxes)
            selector(textBox).OnTextUpdated += handler;
    }

    /// <summary>
    /// Remove <paramref name="handler"/> from all <see cref="ITextBox.OnTextUpdated"/> events in <paramref name="textBoxes"/> after being transformed by a <paramref name="selector"/>.
    /// </summary>
    public static void UnsubscribeAll<T, TTextBox>(IEnumerable<T> textBoxes, Func<T, TTextBox> selector, TextUpdated handler) where TTextBox : ITextBox
    {
        foreach (T textBox in textBoxes)
            selector(textBox).OnTextUpdated -= handler;
    }
    /// <summary>
    /// Create an array of elements using a factory.
    /// </summary>
    public static T[] CreateArray<T>(Func<int, T> factory, int start, int length = -1, int to = -1)
    {
        length = UnturnedUIUtility.ResolveLength(start, length, to);

        if (length == 0)
            return Array.Empty<T>();

        T[] elems = new T[length];

        for (int i = 0; i < length; ++i)
            elems[i] = factory(i + start);

        return elems;
    }

    /// <summary>
    /// Create an array of elements from a format string.
    /// </summary>
    public static T[] CreateArray<T>(string format, int start, int length = -1, int to = -1)
        => CreateArray<T>(GlobalLogger.Instance, format, start, length, to);

    /// <summary>
    /// Create an array of elements from a format string.
    /// </summary>
    public static T[] CreateArray<T>(ILogger logger, string pathFormat, int start, int length = -1, int to = -1)
    {
        length = UnturnedUIUtility.ResolveLength(start, length, to);

        if (length == 0)
            return Array.Empty<T>();

        T[] rtnArray = new T[length];
        Type type = typeof(T);

        if (TryInitializePrimitives(logger, type, rtnArray, pathFormat, start))
            return rtnArray;

        ReadOnlyMemory<char> fullName = pathFormat.AsMemory();
        ReadOnlySpan<char> baseName = UnturnedUIUtility.GetNameFromPathOrName(fullName).Span;
        ReadOnlySpan<char> basePath = UnturnedUIUtility.GetPathFromPathOrName(fullName).Span;
        PatternTypeInfo typeInfo;
        lock (TypeInfo)
        {
            if (!TypeInfo.TryGetValue(type, out typeInfo))
            {
                typeInfo = new PatternTypeInfo(type, null);
                TypeInfo.Add(type, typeInfo);
            }
        }

        for (int i = 0; i < length; ++i)
        {
            rtnArray[i] = (T)InitializeTypeInfo(logger, typeof(T), typeInfo, UnturnedUIUtility.QuickFormat(basePath, i + start, 0), UnturnedUIUtility.QuickFormat(baseName, i + start, 0), i + start);
        }

        return rtnArray;
    }

    /// <summary>
    /// Create an array of elements from a format string.
    /// </summary>
    public static T Create<T>(string path)
    {
        return Create<T>(GlobalLogger.Instance, path);
    }

    /// <summary>
    /// Create an array of elements from a format string.
    /// </summary>
    public static T Create<T>(ILogger logger, string path)
    {
        Type type = typeof(T);

        object? obj = TryInitializePrimitive(logger, type, path);
        if (obj != null)
        {
            return (T)obj;
        }

        ReadOnlyMemory<char> fullName = path.AsMemory();
        ReadOnlyMemory<char> baseName = UnturnedUIUtility.GetNameFromPathOrName(fullName);
        ReadOnlyMemory<char> basePath = UnturnedUIUtility.GetPathFromPathOrName(fullName);
        PatternTypeInfo typeInfo;
        lock (TypeInfo)
        {
            if (!TypeInfo.TryGetValue(type, out typeInfo))
            {
                typeInfo = new PatternTypeInfo(type, null);
                TypeInfo.Add(type, typeInfo);
            }
        }

        return (T)InitializeTypeInfo(logger, typeof(T), typeInfo, basePath.Span, baseName.Span);
    }
    internal static bool IsPrimitive(Type checkedType)
    {
        return typeof(UnturnedUIElement) == checkedType || checkedType.IsSubclassOf(typeof(UnturnedUIElement));
    }
    private static bool TryInitializePrimitives(ILogger logger, Type type, Array array, string format, int start)
    {
        if (!IsPrimitive(type))
            return false;

        for (int i = 0; i < array.Length; ++i)
        {
            array.SetValue(Activator.CreateInstance(type, logger, UnturnedUIUtility.QuickFormat(format, i + start, 0)), i);
        }

        return true;
    }
    private static object? TryInitializePrimitive(ILogger logger, Type type, string path)
    {
        return !IsPrimitive(type)
            ? null
            : Activator.CreateInstance(type, logger, path);
    }
    private static unsafe void ResolveVariablePath(PatternVariableInfo variable, ref ReadOnlySpan<char> basePath, ref ReadOnlySpan<char> baseName, int index = -1)
    {
        if (!string.IsNullOrEmpty(variable.AdditionalPath))
        {
            basePath = CombinePath(basePath, index == -1 ? variable.AdditionalPath : UnturnedUIUtility.QuickFormat(variable.AdditionalPath, index, 0));
        }

        string pattern = variable.Pattern;
        if (index != -1)
        {
            pattern = UnturnedUIUtility.QuickFormat(variable.Pattern, index, 0);
        }

        if (variable.PatternFormatMode is FormatMode.Suffix or FormatMode.Prefix)
        {
            if (pattern.Length == 0)
                return;

            if (baseName.Length == 0)
                baseName = pattern;
        }

        switch (variable.PatternFormatMode)
        {
            case FormatMode.Suffix:
                CreateBaseNameState state = default;
                state.BaseNameLen = baseName.Length;
                state.Pattern = pattern;
                fixed (char* namePtr = baseName)
                {
                    state.BaseNamePtr = namePtr;
                    baseName = string.Create(baseName.Length + pattern.Length, state, (span, state) =>
                    {
                        ReadOnlySpan<char> baseName = new ReadOnlySpan<char>(state.BaseNamePtr, state.BaseNameLen);
                        baseName.CopyTo(span);
                        state.Pattern.AsSpan().CopyTo(span[baseName.Length..]);
                    });
                }

                break;

            case FormatMode.Prefix:
                state.BaseNameLen = baseName.Length;
                state.Pattern = pattern;
                fixed (char* namePtr = baseName)
                {
                    state.BaseNamePtr = namePtr;
                    baseName = string.Create(baseName.Length + pattern.Length, state, (span, state) =>
                    {
                        ReadOnlySpan<char> baseName = new ReadOnlySpan<char>(state.BaseNamePtr, state.BaseNameLen);
                        state.Pattern.AsSpan().CopyTo(span);
                        baseName.CopyTo(span[state.Pattern.Length..]);
                    });
                }

                break;

            case FormatMode.Replace:
                baseName = pattern.Length == 0 ? string.Empty : pattern.AsSpan();
                break;

            case FormatMode.Format:
                basePath = UnturnedUIUtility.QuickFormat(basePath, pattern, variable.PatternFormatIndex, variable.PatternCleanJoin);
                baseName = UnturnedUIUtility.QuickFormat(baseName, pattern, variable.PatternFormatIndex, variable.PatternCleanJoin);
                break;
        }
    }
    private unsafe struct CreateBaseNameState
    {
        public char* BaseNamePtr;
        public int BaseNameLen;
        public string Pattern;
    }

    private static object InitializeTypeInfo(ILogger logger, Type rootType, PatternTypeInfo typeInfo, ReadOnlySpan<char> basePath, ReadOnlySpan<char> baseName, int index = -1)
    {
        object obj = Activator.CreateInstance(typeInfo.Type);
        foreach (PatternVariableInfo variable in typeInfo.Variables)
        {
            ReadOnlySpan<char> baseVarPath = basePath, baseVarName = baseName;
            if (variable is { IsArray: true, ElementType: not null })
            {
                ResolveVariablePath(variable, ref baseVarPath, ref baseVarName);

                Array array = Array.CreateInstance(variable.ElementType, variable.ArrayLength);
                if (IsPrimitive(variable.ElementType))
                {
                    string format = CombinePath(baseVarPath, baseVarName);
                    TryInitializePrimitives(logger, variable.ElementType, array, format, variable.ArrayStart);
                }
                else
                {
                    for (int i = 0; i < array.Length; ++i)
                    {
                        array.SetValue(InitializeTypeInfo(logger, rootType, variable.NestedType!, UnturnedUIUtility.QuickFormat(baseVarPath, i + variable.ArrayStart, 0), UnturnedUIUtility.QuickFormat(baseVarName, i + variable.ArrayStart, 0), i + variable.ArrayStart), i);
                    }
                }
                variable.Variable.SetValue(obj, array);
                continue;
            }

            ResolveVariablePath(variable, ref baseVarPath, ref baseVarName, index);

            if (variable.NestedType != null)
            {
                variable.Variable.SetValue(obj, InitializeTypeInfo(logger, rootType, variable.NestedType, baseVarPath, baseVarName));
            }
            else
            {
                object? prim = TryInitializePrimitive(logger, variable.MemberType, CombinePath(baseVarPath, baseVarName));
                if (prim == null)
                    throw new InvalidOperationException($"Failed to initialize type {Accessor.ExceptionFormatter.Format(typeInfo.Type)} when creating {Accessor.ExceptionFormatter.Format(rootType)}.");

                variable.Variable.SetValue(obj, prim);
            }
        }

        return obj;
    }
    private static unsafe string CombinePath(ReadOnlySpan<char> path, ReadOnlySpan<char> name)
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
    private unsafe struct CombinePathState
    {
        public char* PathPtr;
        public char* NamePtr;
        public int PathLen;
        public int NameLen;
    }

    private static ReadOnlySpan<char> TrimSlashes(ReadOnlySpan<char> span, bool front, bool end)
    {
        if (!front && !end)
            return span;

        int startInd = 0;
        int endInd = span.Length - 1;
        if (endInd == -1)
            return span;

        if (startInd == endInd)
        {
            return span[0] == '/' ? default : span;
        }

        if (front)
        {
            while (startInd < span.Length && span[startInd] == '/')
                ++startInd;
        }
        if (end)
        {
            while (endInd >= 0 && span[endInd] == '/')
                --endInd;
        }

        if (endInd < 0 || startInd >= span.Length)
            return default;

        return span.Slice(startInd, endInd - startInd + 1);
    }
}

/// <summary>
/// Defines how a formatted name is treated.
/// </summary>
public enum FormatMode
{
    /// <summary>
    /// Value is inserted after the parent's name.
    /// </summary>
    /// <remarks>This is the default value.</remarks>
    Suffix,

    /// <summary>
    /// Value is inserted before the parent's name.
    /// </summary>
    Prefix,

    /// <summary>
    /// Value replaces the parent's name.
    /// </summary>
    Replace,

    /// <summary>
    /// Value is formatted into the parent's name using <c>{n}</c> placeholders.
    /// </summary>
    Format,

    /// <summary>
    /// No changes are made to the parent's name.
    /// </summary>
    None
}