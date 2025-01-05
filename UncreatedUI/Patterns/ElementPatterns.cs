using DanielWillett.ReflectionTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Uncreated.Framework.UI.Presets;

namespace Uncreated.Framework.UI.Patterns;

/// <summary>
/// Create arrays of elements based on a pattern defined by decorating propeties and fields with a <see cref="PatternAttribute"/>.
/// </summary>
public static class ElementPatterns
{
    internal static readonly Dictionary<Type, PatternTypeInfo> TypeInfo = new Dictionary<Type, PatternTypeInfo>();
    internal static readonly Dictionary<Type, ConstructorInfo> PrimitiveConstructors = new Dictionary<Type, ConstructorInfo>();

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
    /// Create an array of elements using a factory method.
    /// </summary>
    /// <param name="start">Starting number of the paths of the returned elements.</param>
    /// <param name="length">Number of elements in the array. Either this or <paramref name="to"/> must be supplied, but not both.</param>
    /// <param name="to">Number of the last element in the array. Either this or <paramref name="start"/> must be supplied, but not both.</param>
    /// <param name="factory">Factory method used to create elements when supplied with the element's number (index + <paramref name="start"/>).</param>
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
    /// <param name="start">Starting number of the paths of the returned elements.</param>
    /// <param name="length">Number of elements in the array. Either this or <paramref name="to"/> must be supplied, but not both.</param>
    /// <param name="to">Number of the last element in the array. Either this or <paramref name="start"/> must be supplied, but not both.</param>
    /// <param name="pathFormat">If the pattern has a root element, this would be the path to the root element. If not, this would be the common path to all elements which are transformed by the pattern attribute. <c>{0}</c> is replaced by the index.</param>
    public static T[] CreateArray<T>(string pathFormat, int start, int length = -1, int to = -1)
    {
        length = UnturnedUIUtility.ResolveLength(start, length, to);

        if (length == 0)
            return Array.Empty<T>();

        T[] rtnArray = new T[length];
        Type type = typeof(T);

        if (IsPrimitive(type, out _))
        {
            if (TryInitializePrimitives(type, new EnumerableWrapper(rtnArray), pathFormat, start, false))
                return rtnArray;
        }

        if (TryInitializePresets(type, rtnArray, pathFormat, start))
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

        List<int> indexStack = new List<int>(3) { -1 };
        for (int i = 0; i < length; ++i)
        {
            indexStack[0] = i + start;
            rtnArray[i] = (T)InitializeTypeInfo(typeof(T), typeInfo, null, null, UnturnedUIUtility.QuickFormat(basePath, i + start, 0), UnturnedUIUtility.QuickFormat(baseName, i + start, 0), indexStack);
        }

        return rtnArray;
    }

    /// <summary>
    /// Create a single class of a pattern at the given <paramref name="path"/>.
    /// </summary>
    /// <param name="path">If the pattern has a root element, this would be the path to the root element. If not, this would be the common path to all elements which are transformed by the pattern attribute.</param>
    public static T Create<T>(string path)
    {
        Type type = typeof(T);

        object? obj = TryInitializePrimitive(type, path, false);
        if (obj != null)
        {
            return (T)obj;
        }

        obj = TryInitializePreset(type, path);
        if (obj != null)
            return (T)obj;

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

        return (T)InitializeTypeInfo(typeof(T), typeInfo, null, null, basePath.Span, baseName.Span, new List<int>());
    }

    internal static bool IsPrimitive(Type checkedType, [MaybeNullWhen(false)] out ConstructorInfo ctor)
    {
        if (PrimitiveConstructors.TryGetValue(checkedType, out ctor))
            return true;

        if (typeof(UnturnedUIElement) != checkedType && !checkedType.IsSubclassOf(typeof(UnturnedUIElement)))
        {
            ctor = null;
            return false;
        }

        ctor = checkedType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, [ typeof(string) ], null);
        if (ctor != null && !ctor.IsIgnored())
        {
            PrimitiveConstructors.Add(checkedType, ctor);
            return true;
        }

        ctor = null;
        return false;

    }

    private static bool TryInitializePrimitives(Type type, EnumerableWrapper wrapper, string format, int start, bool textBoxUseData)
    {
        if (!IsPrimitive(type, out ConstructorInfo? ctor))
            return false;

        object[] parameters = new object[1];
        for (int i = 0; i < wrapper.Size; ++i)
        {
            parameters[0] = UnturnedUIUtility.QuickFormat(format, i + start, 0);
            object prim = ctor.Invoke(parameters);
            if (prim is ITextBox t)
                t.UseData = textBoxUseData;

            wrapper.Set(prim, i);
        }

        return true;
    }

    private static object? TryInitializePrimitive(Type type, string path, bool textBoxUseData)
    {
        if (!IsPrimitive(type, out ConstructorInfo? ctor))
            return null;

        object prim = ctor.Invoke([ path ]);
        if (prim is ITextBox t)
            t.UseData = textBoxUseData;

        return prim;    
    }

    private static bool TryInitializePresets(Type type, Array array, string format, int start)
    {
        if (!TryGetPresetConstructor(type, 1, out ConstructorInfo? ctor, out object[]? parameters))
            return false;

        for (int i = 0; i < array.Length; ++i)
        {
            parameters[0] = UnturnedUIUtility.QuickFormat(format, i + start, 0);
            array.SetValue(ctor.Invoke(parameters), i);
        }

        return true;
    }
    private static object? TryInitializePreset(Type type, string path)
    {
        if (!TryGetPresetConstructor(type, 1, out ConstructorInfo? ctor, out object[]? parameters))
            return null;

        parameters[0] = path;
        return ctor.Invoke(parameters);
    }

    private static bool TryGetPresetConstructor(Type type, int numStrings, [MaybeNullWhen(false)] out ConstructorInfo constructorInfo, [MaybeNullWhen(false)] out object[] args)
    {
        ConstructorInfo[] ctors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

        ConstructorInfo? minArgs = null;
        int minArgsCt = 0;
        for (int i = 0; i < ctors.Length; ++i)
        {
            ConstructorInfo ctor = ctors[i];
            ParameterInfo[] parameters = ctor.GetParameters();

            if (parameters.Length < numStrings || parameters.Any(x => x.ParameterType != typeof(string)))
                continue;

            if (parameters.Length == numStrings)
            {
                constructorInfo = ctor;
                args = new object[parameters.Length];
                return true;
            }

            if (minArgs == null || parameters.Length < minArgsCt)
            {
                minArgs = ctor;
                minArgsCt = parameters.Length;
            }
        }

        if (minArgs == null)
        {
            constructorInfo = null;
            args = null;
            return false;
        }

        constructorInfo = minArgs;
        args = new object[minArgsCt];
        return true;
    }

    private static unsafe void ResolveVariablePath(PatternVariableInfo variable, ref ReadOnlySpan<char> basePath, ref ReadOnlySpan<char> baseName, List<int>? indexStack, int presetCtorIndex = -1)
    {
        if (variable.RootInfo != null)
        {
            ReadOnlySpan<char> newBasePath = basePath, newBaseName = baseName;
            ResolveVariablePath(variable.RootInfo, ref newBasePath, ref newBaseName, indexStack);

            basePath = newBaseName.Length == 0 ? newBasePath : UnturnedUIUtility.CombinePath(newBasePath, newBaseName);
        }

        if (!string.IsNullOrEmpty(variable.AdditionalPath))
        {
            string additionalPath = variable.AdditionalPath;

            if (indexStack is { Count: > 0 })
            {
                Span<char> fmt = stackalloc char[UnturnedUIUtility.CountDigits(indexStack.Count) + 2];
                fmt[0] = '{';
                for (int i = indexStack.Count; i >= 0; --i)
                {
                    int formatIndex = indexStack.Count - i;
                    formatIndex.TryFormat(fmt[1..], out _, "F0", CultureInfo.InvariantCulture);
                    fmt[^1] = '}';

                    if (additionalPath.AsSpan().IndexOf(fmt, StringComparison.Ordinal) != -1)
                    {
                        additionalPath = UnturnedUIUtility.QuickFormat(additionalPath, indexStack[i], formatIndex);
                    }
                }
            }

            ReadOnlySpan<char> pathSpan = additionalPath.AsSpan();
            if (pathSpan.Length != 0)
            {
                if (pathSpan.Length > 0 && pathSpan[0] is '.' or '~')
                {
                    basePath = UnturnedUIUtility.ResolveRelativePath(basePath, pathSpan);
                }
                else
                {
                    basePath = UnturnedUIUtility.CombinePath(basePath, pathSpan);
                }
            }
        }

        string pattern = variable.Pattern;
        if (indexStack is { Count: > 0 })
        {
            Span<char> fmt = stackalloc char[UnturnedUIUtility.CountDigits(indexStack.Count) + 2];
            fmt[0] = '{';
            for (int i = indexStack.Count - 1; i >= 0; --i)
            {
                int formatIndex = indexStack.Count - i;
                formatIndex.TryFormat(fmt[1..], out _, "F0", CultureInfo.InvariantCulture);
                fmt[^1] = '}';

                if (pattern.AsSpan().IndexOf(fmt, StringComparison.Ordinal) != -1)
                {
                    pattern = UnturnedUIUtility.QuickFormat(pattern, indexStack[i], formatIndex);
                }
            }
        }

        if (variable.PatternFormatMode is FormatMode.Suffix or FormatMode.Prefix)
        {
            if (pattern.Length == 0)
                return;

            if (baseName.Length == 0)
            {
                baseName = pattern;
                return;
            }
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

    private static object InitializeTypeInfo(Type rootType, PatternTypeInfo typeInfo, PatternVariableInfo? selfVariable, object? parentInstance, ReadOnlySpan<char> basePath, ReadOnlySpan<char> baseName, List<int> indexStack)
    {
        object? obj = null;
        if (selfVariable != null && parentInstance != null && selfVariable.Variable.CanGet)
        {
            obj = selfVariable.Variable.GetValue(parentInstance);
        }

        if (obj == null)
        {
            try
            {
                obj = Activator.CreateInstance(typeInfo.Type);
            }
            catch (Exception ex)
            {
                throw new ElementPatternCreationException(string.Format(Properties.Resources.Exception_FailedToCreatePatternClass, Accessor.ExceptionFormatter.Format(typeInfo.Type)), ex);
            }
        }

        foreach (PatternVariableInfo variable in typeInfo.Variables)
        {
            ConstructorInfo? presetCtor;
            object?[]? args;
            ReadOnlySpan<char> baseVarPath = basePath, baseVarName = baseName;
            if (variable is { IsArray: true, ElementType: not null })
            {
                ResolveVariablePath(variable, ref baseVarPath, ref baseVarName, indexStack);

                EnumerableWrapper array = new EnumerableWrapper(obj, variable, variable.ArrayLength);
                if (IsPrimitive(variable.ElementType, out _))
                {
                    string format = UnturnedUIUtility.CombinePath(baseVarPath, baseVarName);
                    TryInitializePrimitives(variable.ElementType, array, format, variable.ArrayStart, variable.TextBoxUseData);
                }
                else if (TryGetPresetConstructor(variable.ElementType, variable.PresetPathsCount, out presetCtor, out args))
                {
                    indexStack.Add(0);
                    for (int i = 0; i < array.Size; ++i)
                    {
                        int number = i + variable.ArrayStart;
                        indexStack[^1] = number;
                        string fmtPath = UnturnedUIUtility.QuickFormat(baseVarPath, number, 0);
                        args[0] = baseVarName.Length == 0 ? fmtPath : UnturnedUIUtility.CombinePath(fmtPath, UnturnedUIUtility.QuickFormat(baseVarName, number, 0));
                        for (int j = 1; j < variable.PresetPathsCount; ++j)
                        {
                            string? path = variable.PresetPaths![j - 1];
                            if (path == null)
                            {
                                args[j] = null;
                                continue;
                            }

                            path = UnturnedUIUtility.QuickFormat(path, number, 0);
                            if (!UnturnedUIUtility.IsRooted(path))
                            {
                                args[j] = path;
                                continue;
                            }

                            ReadOnlySpan<char> varPath = fmtPath;
                            ReadOnlySpan<char> varName = baseName;

                            ResolveVariablePath(variable, ref varPath, ref varName, indexStack, j - 1);
                            args[j] = varName.Length == 0 ? new string(varPath) : UnturnedUIUtility.CombinePath(varPath, varName);
                        }

                        object value = presetCtor.Invoke(args);
                        if (value is ITextBox t)
                            t.UseData = variable.TextBoxUseData;
                        array.Set(value, i);
                    }
                    indexStack.RemoveAt(indexStack.Count - 1);
                }
                else
                {
                    indexStack.Add(0);
                    for (int i = 0; i < array.Size; ++i)
                    {
                        int number = i + variable.ArrayStart;
                        indexStack[^1] = number;
                        object value = InitializeTypeInfo(
                            rootType,
                            variable.NestedType!,
                            null,
                            null,
                            UnturnedUIUtility.QuickFormat(baseVarPath, number, 0),
                            UnturnedUIUtility.QuickFormat(baseVarName, number, 0),
                            indexStack
                        );

                        if (value is ITextBox t)
                            t.UseData = variable.TextBoxUseData;
                        array.Set(value, i);
                    }
                    indexStack.RemoveAt(indexStack.Count - 1);
                }
                
                if (array.NeedsAssign)
                    variable.Variable.SetValue(obj, array.GetValue());
                continue;
            }

            object? existingValue = variable.Variable.CanGet ? variable.Variable.GetValue(obj) : null;

            ResolveVariablePath(variable, ref baseVarPath, ref baseVarName, indexStack);

            if (TryGetPresetConstructor(variable.CreatedType, variable.PresetPathsCount, out presetCtor, out args))
            {
                if (existingValue != null)
                    continue;

                args[0] = baseVarName.Length == 0 ? new string(baseVarPath) : UnturnedUIUtility.CombinePath(baseVarPath, baseVarName);
                for (int j = 1; j < variable.PresetPathsCount; ++j)
                {
                    string? path = variable.PresetPaths![j - 1];
                    if (path == null || !UnturnedUIUtility.IsRooted(path))
                    {
                        args[j] = path;
                        continue;
                    }

                    ReadOnlySpan<char> varPath = baseVarPath;
                    ReadOnlySpan<char> varName = baseName;

                    ResolveVariablePath(variable, ref varPath, ref varName, indexStack, j - 1);
                    args[j] = varName.Length == 0 ? new string(varPath) : UnturnedUIUtility.CombinePath(varPath, varName);
                }

                object value = presetCtor.Invoke(args);
                if (value is ITextBox t)
                    t.UseData = variable.TextBoxUseData;
                variable.Variable.SetValue(obj, value);
            }
            else if (variable.NestedType != null)
            {
                object value = InitializeTypeInfo(rootType, variable.NestedType, variable, obj, baseVarPath, baseVarName, indexStack);
                if (value is ITextBox t)
                    t.UseData = variable.TextBoxUseData;
                variable.Variable.SetValue(obj, value);
            }
            else
            {
                if (existingValue != null)
                    continue;

                object? prim = TryInitializePrimitive(
                    variable.CreatedType,
                    baseVarName.Length == 0
                        ? new string(baseVarPath)
                        : UnturnedUIUtility.CombinePath(baseVarPath, baseVarName),
                    variable.TextBoxUseData
                );

                if (prim == null)
                    throw new ElementPatternCreationException(string.Format(Properties.Resources.Exception_FailureCreatingPrimitive, Accessor.ExceptionFormatter.Format(typeInfo.Type), Accessor.ExceptionFormatter.Format(rootType)));
                
                variable.Variable.SetValue(obj, prim);
            }
        }

        return obj;
    }

    private abstract class CollectionWrapper
    {
        public abstract object Value { get; }
        public abstract void Add(object v);
    }
    private class CollectionWrapper<T> : CollectionWrapper
    {
        private readonly ICollection<T> _collection;

        public override object Value => _collection;

        public CollectionWrapper(ICollection<T> collection, int capacity)
        {
            _collection = collection;
            if (_collection is List<T> l)
                l.Capacity = capacity;
        }

        public override void Add(object v)
        {
            _collection.Add((T)v);
        }
    }

    private readonly struct EnumerableWrapper
    {
        public readonly int Size;
        public readonly bool NeedsAssign;

        private readonly int _type;
        private readonly object _collection;
        public void Set(object value, int index)
        {
            switch (_type)
            {
                case 0:
                    ((Array)_collection).SetValue(value, index);
                    break;

                case 1:
                    ((IList)_collection).Add(value);
                    break;

                default:
                    ((CollectionWrapper)_collection).Add(value);
                    break;
            }
        }

        public object GetValue()
        {
            return _type == 2 ? ((CollectionWrapper)_collection).Value : _collection;
        }

        public EnumerableWrapper(Array array)
        {
            Size = array.Length;
            _collection = array;
        }

        public EnumerableWrapper(object instance, PatternVariableInfo variable, int count)
        {
            Size = count;
            Type elementType = variable.ElementType!;
            Type enumerableType = variable.CreatedType;

            object? existing = variable.Variable.GetValue(instance);

            if (existing == null)
            {
                NeedsAssign = true;
                if (enumerableType.IsInterface)
                {
                    if (enumerableType == typeof(IEnumerable) ||
                        enumerableType == typeof(ICollection) ||
                        enumerableType == typeof(IReadOnlyList<>).MakeGenericType(elementType) ||
                        enumerableType == typeof(IReadOnlyCollection<>).MakeGenericType(elementType))
                    {
                        _collection = Array.CreateInstance(elementType, count);
                    }
                    else if (elementType == typeof(IList))
                    {
                        _collection = new ArrayList(count);
                        _type = 1;
                    }
                    else if (enumerableType == typeof(IList)
                             || enumerableType == typeof(IList<>).MakeGenericType(elementType))
                    {
                        _collection = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType), count);
                        _type = 1;
                    }
                    else
                    {
                        throw new ElementPatternCreationException(string.Format(Properties.Resources.Exception_UnknownEnumerableType, Accessor.ExceptionFormatter.Format(variable.Variable.DeclaringType!), variable.Variable.Format(Accessor.ExceptionFormatter)));
                    }

                    return;
                }

                // array (null)
                if (enumerableType.IsArray)
                {
                    _collection = Array.CreateInstance(elementType, count);
                }
                else if (typeof(IList).IsAssignableFrom(enumerableType))
                {
                    if (enumerableType.IsGenericType && enumerableType.GetGenericTypeDefinition() == typeof(List<>))
                        _collection = Activator.CreateInstance(enumerableType, count);
                    else
                        _collection = Activator.CreateInstance(enumerableType);
                    _type = 1;
                }
                else if (typeof(ICollection<>).MakeGenericType(elementType).IsAssignableFrom(enumerableType))
                {
                    _collection = Activator.CreateInstance(typeof(CollectionWrapper<>).MakeGenericType(elementType), Activator.CreateInstance(enumerableType), count);
                    _type = 2;
                }
                else
                {
                    throw new ElementPatternCreationException(string.Format(Properties.Resources.Exception_UnknownEnumerableType, Accessor.ExceptionFormatter.Format(variable.Variable.DeclaringType!), variable.Variable.Format(Accessor.ExceptionFormatter)));
                }
            }
            else
            {
                _collection = existing;
                switch (existing)
                {
                    case Array arr:
                        if (arr.Length < count)
                            throw new ElementPatternCreationException(string.Format(Properties.Resources.Exception_FillEnumerableArrayTooSmall, Accessor.ExceptionFormatter.Format(variable.Variable.DeclaringType!), variable.Variable.Format(Accessor.ExceptionFormatter)));
                        _type = 0;
                        break;

                    case IList:
                        _type = 1;
                        break;

                    default:
                        if (!typeof(ICollection<>).MakeGenericType(elementType).IsInstanceOfType(existing))
                            throw new ElementPatternCreationException(string.Format(Properties.Resources.Exception_FillEnumerableNotValidType, Accessor.ExceptionFormatter.Format(variable.Variable.DeclaringType!), variable.Variable.Format(Accessor.ExceptionFormatter)));
                        _type = 2;
                        _collection = Activator.CreateInstance(typeof(CollectionWrapper<>).MakeGenericType(elementType), existing, count);
                        break;
                }
            }
        }
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