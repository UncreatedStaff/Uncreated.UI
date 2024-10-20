using DanielWillett.ReflectionTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Uncreated.Framework.UI.Patterns;
internal sealed class PatternVariableInfo
{
    public readonly Type MemberType;
    public readonly Type? ElementType;
    public readonly IVariable Variable;
    public readonly FormatMode PatternFormatMode;
    public readonly string Pattern;
    public readonly string?[]? PresetPaths;
    public readonly int PresetPathsCount;
    public readonly bool IsRoot;
    public readonly bool UnderRoot;
    public readonly char? PatternCleanJoin;
    public readonly string? AdditionalPath;
    public readonly int PatternFormatIndex;
    public readonly int ArrayStart;
    public readonly int ArrayLength;
    public readonly bool IsArray;
    public readonly PatternTypeInfo? NestedType;
    public PatternVariableInfo? RootInfo;
    public PatternVariableInfo(IVariable variable, PatternTypeInfo? rootType, PatternTypeInfo ownerType)
    {
        Variable = variable;
        if (variable.Member.TryGetAttributeSafe(out PatternAttribute pattern, inherit: true))
        {
            Pattern = pattern.Pattern ?? string.Empty;
            PatternCleanJoin = pattern.CleanJoin;
            AdditionalPath = pattern.AdditionalPath;
            PatternFormatMode = pattern.Mode;
            PatternFormatIndex = pattern.FormatIndex;
            IsRoot = pattern.Root;
            UnderRoot = !IsRoot && pattern.UnderRoot;
            PresetPaths = pattern.PresetPaths;
            PresetPathsCount = PresetPaths == null ? 1 : (PresetPaths.Length + 1); 
        }
        else
        {
            Pattern = variable.Member.Name;
            PatternFormatMode = FormatMode.Suffix;
            PatternFormatIndex = 1;
            PresetPathsCount = 1;
        }

        if (variable.Member.TryGetAttributeSafe(out ArrayPatternAttribute arrayPattern, inherit: true))
        {
            if (IsRoot)
                throw new NotSupportedException($"Arrays can not be root objects (like in {ToString()}).");

            IsArray = true;
            ArrayStart = arrayPattern.Start;
            ArrayLength = arrayPattern.Length;
        }

        MemberType = Variable.MemberType;
        if (MemberType.IsArray)
        {
            if (MemberType.GetArrayRank() != 1)
            {
                throw new NotSupportedException($"Multi-dimensional arrays are not supported (like in {ToString()}).");
            }

            ElementType = MemberType.GetElementType();
        }
        else
        {
            Type[] intx = MemberType.GetInterfaces();
            bool hasTypeUnsafeEnumerable = false;
            Type? enumerableType = null;
            for (int i = 0; i < intx.Length; ++i)
            {
                Type intxType = intx[i];
                if (intxType == typeof(IEnumerable))
                    hasTypeUnsafeEnumerable = true;
                else if (!intxType.IsGenericType || intxType.GetGenericTypeDefinition() != typeof(IEnumerable<>))
                    continue;

                Type genArg = intxType.GetGenericArguments()[0];
                if (enumerableType == null || !genArg.IsAssignableFrom(enumerableType))
                    enumerableType = genArg;
            }

            if (enumerableType == null)
            {
                ElementType = hasTypeUnsafeEnumerable
                    ? typeof(object)
                    : null;
            }
            else
            {
                ElementType = enumerableType;
            }
        }

        if (ElementType != null && !IsArray)
        {
            throw new NotSupportedException($"Arrays must be decorated with a ArrayPatternAttribute (like in {ToString()}).");
        }

        if (IsArray && (ElementType == typeof(object) || ElementType == typeof(ValueType)))
        {
            throw new NotSupportedException($"Element type must not be a system base type (like in {ToString()}).");
        }

        if (IsArray && ElementType == null)
        {
            throw new NotSupportedException($"Non-array variables can not be decorated with ArrayPatternAttribute (like in {ToString()}).");
        }

        if (!IsArray && (MemberType == typeof(object) || MemberType == typeof(ValueType)))
        {
            throw new NotSupportedException($"Member type must not be a system base type (like in {ToString()}).");
        }

        if (ElementPatterns.IsPrimitive(MemberType) || ElementType != null && ElementPatterns.IsPrimitive(ElementType))
            return;

        Type valueType = IsArray ? ElementType! : MemberType;

        lock (ElementPatterns.TypeInfo)
        {
            NestedType = ElementPatterns.TypeInfo.TryGetValue(valueType, out PatternTypeInfo existingTypeInfo)
                ? existingTypeInfo
                : FindInRootType(rootType, ownerType, valueType);
        }

        if (NestedType != null)
            return;

        NestedType = new PatternTypeInfo(valueType, rootType);

        lock (ElementPatterns.TypeInfo)
        {
            if (!ElementPatterns.TypeInfo.TryGetValue(NestedType.Type, out PatternTypeInfo nestedType))
            {
                ElementPatterns.TypeInfo.Add(NestedType.Type, NestedType);
            }
            else
            {
                NestedType = nestedType;
            }
        }
    }
    private static PatternTypeInfo? FindInRootType(PatternTypeInfo? rootType, PatternTypeInfo? ownerType, Type memberType)
    {
        if (ownerType != null && ownerType.Type == memberType)
            return ownerType;

        if (rootType == null)
            return null;

        if (rootType.Type == memberType)
            return rootType;

        foreach (PatternVariableInfo variableInfo in rootType.Variables)
        {
            if (variableInfo.NestedType == null)
                continue;

            PatternTypeInfo? typeInfo = FindInRootType(variableInfo.NestedType, null, memberType);
            if (typeInfo != null)
                return typeInfo;
        }

        return null;
    }

    public override string ToString() => Variable.Member switch
    {
        FieldInfo f => Accessor.ExceptionFormatter.Format(f),
        PropertyInfo p => Accessor.ExceptionFormatter.Format(p),
        _ => Variable.Member.Name
    };
}
