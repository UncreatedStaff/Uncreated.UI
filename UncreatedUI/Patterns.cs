using DanielWillett.ReflectionTools;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Uncreated.Framework.UI.Presets;

namespace Uncreated.Framework.UI;

/// <summary>
/// Create arrays of elements based on a pattern.
/// </summary>
public static class ElementPatterns
{
    private static readonly ConcurrentDictionary<Type, PatternTypeInfo> TypeInfo = new ConcurrentDictionary<Type, PatternTypeInfo>();

    /// <summary>
    /// Create an array of elements using a factory.
    /// </summary>
    public static T[] CreateArray<T>(Func<int, T> factory, int start, int length = -1, int to = -1)
    {
        length = ResolveLength(start, length, to);

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
    public static T[] CreateArray<T>(ILogger logger, string format, int start, int length = -1, int to = -1)
    {
        length = ResolveLength(start, length, to);

        if (length == 0)
            return Array.Empty<T>();

        T[] elems = new T[length];
        Type type = typeof(T);

        if (TryInitializePrimitives(logger, type, elems, format, start))
            return elems;

        PatternTypeInfo info = PatternTypeInfo.GetTypeInfo(typeof(T));
        info.InitializeMany(logger, elems, start, format);
        return elems;
    }

    /// <summary>
    /// Add <paramref name="handler"/> to all <see cref="UnturnedButton.OnClicked"/> events in <paramref name="buttons"/>.
    /// </summary>
    public static void SubscribeAll(IEnumerable<UnturnedButton> buttons, ButtonClicked handler)
    {
        foreach (UnturnedButton button in buttons)
            button.OnClicked += handler;
    }

    /// <summary>
    /// Remove <paramref name="handler"/> from all <see cref="UnturnedButton.OnClicked"/> events in <paramref name="buttons"/>.
    /// </summary>
    public static void UnsubscribeAll(IEnumerable<UnturnedButton> buttons, ButtonClicked handler)
    {
        foreach (UnturnedButton button in buttons)
            button.OnClicked -= handler;
    }

    /// <summary>
    /// Add <paramref name="handler"/> to all <see cref="UnturnedTextBox.OnTextUpdated"/> events in <paramref name="textBoxes"/>.
    /// </summary>
    public static void SubscribeAll(IEnumerable<UnturnedTextBox> textBoxes, TextUpdated handler)
    {
        foreach (UnturnedTextBox textBox in textBoxes)
            textBox.OnTextUpdated += handler;
    }

    /// <summary>
    /// Remove <paramref name="handler"/> from all <see cref="UnturnedTextBox.OnTextUpdated"/> events in <paramref name="textBoxes"/>.
    /// </summary>
    public static void UnsubscribeAll(IEnumerable<UnturnedTextBox> textBoxes, TextUpdated handler)
    {
        foreach (UnturnedTextBox textBox in textBoxes)
            textBox.OnTextUpdated -= handler;
    }

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
    private static int ResolveLength(int start, int length, int to)
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
    private static bool TryInitializePrimitives(ILogger logger, Type type, Array array, string format, int start)
    {
        if (typeof(UnturnedUIElement).IsAssignableFrom(type))
        {
            for (int i = 0; i < array.Length; ++i)
                array.SetValue(Activator.CreateInstance(type, UnturnedUIUtility.QuickFormat(format, (i + start).ToString(CultureInfo.InvariantCulture), 0)), i);

            return true;
        }
        if (typeof(ICollection).IsAssignableFrom(type))
        {
            logger.LogError("Invalid type (nested enumerables) when creating pattern array: {0}.", type);
            return true;
        }

        return false;
    }
    private class PatternTypeInfo
    {
        private readonly Type _type;
        private readonly PatternFieldInfo[] _fields;
        private PatternTypeInfo(Type type)
        {
            _type = type;
            TypeInfo[type] = this;
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            List<PatternFieldInfo> list = new List<PatternFieldInfo>(fields.Length + properties.Length);
            for (int i = 0; i < fields.Length; ++i)
            {
                FieldInfo field = fields[i];
                RecursiveProcess(field, list);
            }
            for (int i = 0; i < properties.Length; ++i)
            {
                PropertyInfo property = properties[i];
                if (property.GetSetMethod(true) != null)
                    RecursiveProcess(property, list);
            }

            _fields = list.ToArray();
        }
        private static void RecursiveProcess(MemberInfo member, ICollection<PatternFieldInfo> list)
        {
            if (member.IsIgnored()) return;
            Type fieldType = member.GetMemberType()!;

            if (typeof(IEnumerable).IsAssignableFrom(fieldType))
            {
                if (Attribute.GetCustomAttribute(member, typeof(UIPatternArrayAttribute)) is not UIPatternArrayAttribute array)
                    return;
                Type elementType = GetElementType(fieldType);
                if (elementType == typeof(object))
                    return;
                
                if (typeof(UnturnedUIElement).IsAssignableFrom(elementType))
                {
                    if (Attribute.GetCustomAttribute(member, typeof(UIPatternAttribute)) is UIPatternAttribute attr)
                        list.Add(new PatternFieldInfo(member, elementType, null, attr, array));
                }
                else if (!typeof(IEnumerable).IsAssignableFrom(elementType))
                {
                    if (Attribute.GetCustomAttribute(member, typeof(UIPatternAttribute)) is not UIPatternAttribute attr)
                        return;

                    PatternTypeInfo typeInfo = GetTypeInfo(elementType);
                    if (typeInfo._fields.Length > 0)
                        list.Add(new PatternFieldInfo(member, elementType, typeInfo, attr, array));
                }

                return;
            }

            if (Attribute.GetCustomAttribute(member, typeof(UIPatternAttribute)) is not UIPatternAttribute attr2)
                return;

            if (typeof(UnturnedUIElement).IsAssignableFrom(fieldType))
                list.Add(new PatternFieldInfo(member, fieldType, null, attr2, null));

            PatternTypeInfo type = GetTypeInfo(fieldType);
            if (type._fields.Length > 0)
                list.Add(new PatternFieldInfo(member, fieldType, type, attr2, null));
        }
        public static PatternTypeInfo GetTypeInfo(Type type)
        {
            return TypeInfo.TryGetValue(type, out PatternTypeInfo info)
                ? info
                : new PatternTypeInfo(type);
        }
        public void InitializeMany(ILogger logger, Array elements, int start, string baseName)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                elements.SetValue(Initialize(logger, UnturnedUIUtility.QuickFormat(baseName, (i + start).ToString(CultureInfo.InvariantCulture), 0)), i);
            }
        }
        private object Initialize(ILogger logger, string baseName)
        {
            object obj = Activator.CreateInstance(_type);
            FillObject(logger, baseName, obj);
            return obj;
        }
        private void FillObject(ILogger logger, string baseName, object obj)
        {
            for (int i = 0; i < _fields.Length; ++i)
            {
                PatternFieldInfo patternField = _fields[i];
                if (patternField.ArrayAttribute != null)
                {
                    Array arr = Array.CreateInstance(patternField.MemberType, patternField.ArrayAttribute.Length);
                    if (patternField.ArrayAttribute.Length > 0)
                    {
                        if (!TryInitializePrimitives(logger, patternField.MemberType, arr, patternField.ResolveName(baseName), patternField.ArrayAttribute.Start))
                            patternField.PatternTypeInfo!.InitializeMany(logger, arr, patternField.ArrayAttribute.Start, patternField.ResolveName(baseName));
                    }

                    patternField.SetValue(obj, arr);
                    continue;
                }

                if (patternField.PatternTypeInfo != null)
                {
                    object val = patternField.PatternTypeInfo.Initialize(logger, patternField.ResolveName(baseName));
                    patternField.SetValue(obj, val);
                }
                else if (typeof(UnturnedUIElement).IsAssignableFrom(patternField.MemberType))
                {
                    object val = Activator.CreateInstance(patternField.MemberType, patternField.ResolveName(baseName));
                    patternField.SetValue(obj, val);
                }
            }
        }
    }
    private class PatternFieldInfo
    {
        private readonly MemberInfo _member;
        private readonly UIPatternAttribute? _attribute;
        public readonly Type MemberType;
        public readonly PatternTypeInfo? PatternTypeInfo;
        public readonly UIPatternArrayAttribute? ArrayAttribute;
        public PatternFieldInfo(MemberInfo member, Type memberType, PatternTypeInfo? patternTypeInfo, UIPatternAttribute? attribute, UIPatternArrayAttribute? arrayAttribute)
        {
            _member = member;
            MemberType = memberType;
            PatternTypeInfo = patternTypeInfo;
            _attribute = attribute;
            ArrayAttribute = arrayAttribute;
        }

        public string ResolveName(string baseName, int? index = null)
        {
            if (_attribute == null || _attribute.Mode == FormatMode.None)
                return baseName;
            string pattern = _attribute.Pattern;
            if (index.HasValue)
                pattern = UnturnedUIUtility.QuickFormat(pattern, index.Value.ToString(CultureInfo.InvariantCulture), 0);
            return _attribute.Mode switch
            {
                FormatMode.Prefix => pattern + baseName,
                FormatMode.Replace => pattern,
                FormatMode.Format => UnturnedUIUtility.QuickFormat(baseName, pattern, _attribute is not { Mode: FormatMode.Format } ? 0 : _attribute.FormatIndex),
                _ => baseName + pattern
            };
        }
        public void SetValue(object instance, object val)
        {
            if (_member is PropertyInfo prop)
                prop.GetSetMethod(true).Invoke(instance, new object[] { val });
            else
                ((FieldInfo)_member).SetValue(instance, val);
        }
    }
    private static Type GetElementType(Type enumerable)
    {
        if (enumerable.IsArray)
            return enumerable.GetElementType()!;

        Type? genEnumerable = enumerable.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        return genEnumerable?.GenericTypeArguments[0] ?? typeof(object);
    }
}
public enum FormatMode
{
    Suffix,
    Prefix,
    Replace,
    Format,
    None
}

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UIPatternAttribute : Attribute
{
    public string Pattern { get; }
    public int Offset { get; set; }
    public FormatMode Mode { get; set; } = FormatMode.Suffix;
    public int FormatIndex { get; set; } = 1;
    public UIPatternAttribute(string pattern)
    {
        Pattern = pattern;
    }
}

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class UIPatternArrayAttribute : Attribute
{
    public int Start { get; }
    public int Length { get; set; }
    public int To
    {
        get => Length + Start - 1;
        set => Length = value - Start + 1;
    }
    public UIPatternArrayAttribute(int start)
    {
        Start = start;
    }
    public UIPatternArrayAttribute(int start, int length)
    {
        Start = start;
        Length = length;
    }
}