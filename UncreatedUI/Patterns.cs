using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Uncreated.Networking;

namespace Uncreated.Framework.UI;
public static class UnturnedUIPatterns
{
    private static readonly ConcurrentDictionary<Type, PatternTypeInfo> TypeInfo = new ConcurrentDictionary<Type, PatternTypeInfo>();
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
    public static T[] CreateArray<T>(string format, int start, int length = -1, int to = -1)
    {
        length = ResolveLength(start, length, to);

        if (length == 0)
            return Array.Empty<T>();

        T[] elems = new T[length];
        Type type = typeof(T);

        if (!TryInitializePrimitives(type, elems, format, start))
        {
            PatternTypeInfo info = PatternTypeInfo.GetTypeInfo(typeof(T));
            info.InitializeMany(elems, start, format);
        }

        return elems;
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
    private static bool TryInitializePrimitives(Type type, Array array, string format, int start)
    {
        if (typeof(UnturnedUIElement).IsAssignableFrom(type))
        {
            for (int i = 0; i < array.Length; ++i)
                array.SetValue(Activator.CreateInstance(type, Util.QuickFormat(format, (i + start).ToString(CultureInfo.InvariantCulture), 0, repeat: true)), i);

            return true;
        }
        if (typeof(ICollection).IsAssignableFrom(type))
        {
            Logging.LogError($"Invalid type (nested enumerables) when creating pattern array: {type}.");
            return true;
        }

        return false;
    }
    private class PatternTypeInfo
    {
        public readonly Type Type;
        public readonly PatternFieldInfo[] Fields;
        private PatternTypeInfo(Type type)
        {
            Type = type;
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

            Fields = list.ToArray();
        }
        private static void RecursiveProcess(MemberInfo member, List<PatternFieldInfo> list)
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
                    if (typeInfo.Fields.Length > 0)
                        list.Add(new PatternFieldInfo(member, elementType, typeInfo, attr, array));
                }

                return;
            }

            if (Attribute.GetCustomAttribute(member, typeof(UIPatternAttribute)) is not UIPatternAttribute attr2)
                return;

            if (typeof(UnturnedUIElement).IsAssignableFrom(fieldType))
                list.Add(new PatternFieldInfo(member, fieldType, null, attr2, null));

            PatternTypeInfo type = GetTypeInfo(fieldType);
            if (type.Fields.Length > 0)
                list.Add(new PatternFieldInfo(member, fieldType, type, attr2, null));
        }
        public static PatternTypeInfo GetTypeInfo(Type type)
        {
            if (TypeInfo.TryGetValue(type, out PatternTypeInfo info))
                return info;
            return new PatternTypeInfo(type);
        }
        public void InitializeMany(Array elements, int start, string baseName)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                elements.SetValue(Initialize(Util.QuickFormat(baseName, (i + start).ToString(CultureInfo.InvariantCulture), 0, repeat: true)), i);
            }
        }
        public object Initialize(string baseName)
        {
            object obj = Activator.CreateInstance(Type);
            FillObject(baseName, obj);
            return obj;
        }
        public void FillObject(string baseName, object obj)
        {
            for (int i = 0; i < Fields.Length; ++i)
            {
                PatternFieldInfo patternField = Fields[i];
                if (patternField.ArrayAttribute != null)
                {
                    Array arr = Array.CreateInstance(patternField.MemberType, patternField.ArrayAttribute.Length);
                    if (patternField.ArrayAttribute.Length > 0)
                    {
                        if (!TryInitializePrimitives(patternField.MemberType, arr, patternField.ResolveName(baseName), patternField.ArrayAttribute.Start))
                            patternField.PatternTypeInfo!.InitializeMany(arr, patternField.ArrayAttribute.Start, patternField.ResolveName(baseName));
                    }

                    patternField.SetValue(obj, arr);
                    continue;
                }

                if (patternField.PatternTypeInfo != null)
                {
                    object val = patternField.PatternTypeInfo.Initialize(patternField.ResolveName(baseName));
                    patternField.SetValue(obj, val);
                }
                else if (typeof(UnturnedUIElement).IsAssignableFrom(patternField.MemberType))
                {
                    object val = Activator.CreateInstance(patternField.MemberType, patternField.ResolveName(baseName));
                    patternField.SetValue(obj, val);
                }
            }
        }
        public class PatternFieldInfo
        {
            public readonly MemberInfo Member;
            public readonly Type MemberType;
            public readonly PatternTypeInfo? PatternTypeInfo;
            public readonly UIPatternAttribute Attribute;
            public readonly UIPatternArrayAttribute? ArrayAttribute;
            public PatternFieldInfo(MemberInfo member, Type memberType, PatternTypeInfo? patternTypeInfo, UIPatternAttribute? attribute, UIPatternArrayAttribute? arrayAttribute)
            {
                Member = member;
                MemberType = memberType;
                PatternTypeInfo = patternTypeInfo;
                Attribute = attribute;
                ArrayAttribute = arrayAttribute;
            }

            public string ResolveName(string baseName, int? index = null)
            {
                if (Attribute == null || Attribute.Mode == FormatMode.None)
                    return baseName;
                string pattern = Attribute.Pattern;
                if (index.HasValue)
                    pattern = Util.QuickFormat(pattern, index.Value.ToString(CultureInfo.InvariantCulture));
                return Attribute.Mode switch
                {
                    FormatMode.Prefix => pattern + baseName,
                    FormatMode.Replace => pattern,
                    FormatMode.Format => Util.QuickFormat(baseName, pattern, Attribute is not { Mode: FormatMode.Format } ? 0 : Attribute.FormatIndex, repeat: true),
                    _ => baseName + pattern
                };
            }
            public void SetValue(object instance, object val)
            {
                if (Member is PropertyInfo prop)
                    prop.GetSetMethod(true).Invoke(instance, new object[] { val });
                else
                    ((FieldInfo)Member).SetValue(instance, val);
            }
        }
    }
    private static Type GetElementType(Type enumerable)
    {
        if (enumerable.IsArray)
            return enumerable.GetElementType()!;

        Type? genEnumerable = enumerable.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        return genEnumerable?.GenericTypeArguments[0] ?? typeof(object);
    }
    public static void SubscribeAll(UnturnedButton[] buttons, ButtonClicked handler)
    {
        for (int i = 0; i < buttons.Length; ++i)
            buttons[i].OnClicked += handler;
    }
    public static void UnsubscribeAll(UnturnedButton[] buttons, ButtonClicked handler)
    {
        for (int i = 0; i < buttons.Length; ++i)
            buttons[i].OnClicked -= handler;
    }
    public static void SubscribeAll(UnturnedTextBox[] textBoxes, TextUpdated handler)
    {
        for (int i = 0; i < textBoxes.Length; ++i)
            textBoxes[i].OnTextUpdated += handler;
    }
    public static void UnsubscribeAll(UnturnedTextBox[] textBoxes, TextUpdated handler)
    {
        for (int i = 0; i < textBoxes.Length; ++i)
            textBoxes[i].OnTextUpdated -= handler;
    }
    public static void SubscribeAll<T>(T[] buttons, Func<T, UnturnedButton> selector, ButtonClicked handler)
    {
        for (int i = 0; i < buttons.Length; ++i)
            selector(buttons[i]).OnClicked += handler;
    }
    public static void UnsubscribeAll<T>(T[] buttons, Func<T, UnturnedButton> selector, ButtonClicked handler)
    {
        for (int i = 0; i < buttons.Length; ++i)
            selector(buttons[i]).OnClicked -= handler;
    }
    public static void SubscribeAll<T>(T[] textBoxes, Func<T, UnturnedTextBox> selector, TextUpdated handler)
    {
        for (int i = 0; i < textBoxes.Length; ++i)
            selector(textBoxes[i]).OnTextUpdated += handler;
    }
    public static void UnsubscribeAll<T>(T[] textBoxes, Func<T, UnturnedTextBox> selector, TextUpdated handler)
    {
        for (int i = 0; i < textBoxes.Length; ++i)
            selector(textBoxes[i]).OnTextUpdated -= handler;
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