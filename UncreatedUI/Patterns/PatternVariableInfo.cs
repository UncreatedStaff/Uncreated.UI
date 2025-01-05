using DanielWillett.ReflectionTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Uncreated.Framework.UI.Patterns;
internal sealed class PatternVariableInfo
{
    public readonly Type CreatedType;
    public readonly Type? ElementType;
    public readonly IVariable Variable;
    public readonly FormatMode PatternFormatMode;
    public readonly string Pattern;
    public readonly string?[]? PresetPaths;
    public readonly int PresetPathsCount;
    public readonly bool IsRoot;
    public readonly bool UnderRoot;
    public readonly bool TextBoxUseData;
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
            TextBoxUseData = pattern.TextBoxUseData;
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
                throw new ElementPatternCreationException(string.Format(Properties.Resources.Exception_ArrayRootObject, Accessor.ExceptionFormatter.Format(ownerType.Type)));

            IsArray = true;
            ArrayStart = arrayPattern.Start;
            ArrayLength = arrayPattern.Length;
        }

        CreatedType = Variable.MemberType;
        if (CreatedType.IsArray)
        {
            if (!CreatedType.IsSZArray)
            {
                throw new ElementPatternCreationException(string.Format(Properties.Resources.Exception_NonSZArray, Accessor.ExceptionFormatter.Format(ownerType.Type)));
            }

            ElementType = CreatedType.GetElementType();
        }
        else if (CreatedType != typeof(string))
        {
            IEnumerable<Type> intx = CreatedType.GetInterfaces();
            if (CreatedType.IsInterface)
                intx = Enumerable.Repeat(CreatedType, 1).Concat(intx);
            bool hasTypeUnsafeEnumerable = false;
            Type? enumerableType = null;
            foreach (Type intxType in intx)
            {
                if (intxType == typeof(IEnumerable))
                {
                    hasTypeUnsafeEnumerable = true;
                    continue;
                }
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
            throw new ElementPatternCreationException(string.Format(Properties.Resources.Exception_ArrayAttributeNotSpecified, Accessor.ExceptionFormatter.Format(ownerType.Type), Variable.Format(Accessor.ExceptionFormatter)));
        }

        if (IsArray && (ElementType == typeof(object) || ElementType == typeof(ValueType)))
        {
            ElementType = typeof(UnturnedUIElement);
        }

        if (IsArray && ElementType == null)
        {
            throw new ElementPatternCreationException(string.Format(Properties.Resources.Exception_ArrayAttributeSpecifiedOnSingular, Accessor.ExceptionFormatter.Format(ownerType.Type), Variable.Format(Accessor.ExceptionFormatter)));
        }

        if (!IsArray && (CreatedType == typeof(object) || CreatedType == typeof(ValueType)))
        {
            CreatedType = typeof(UnturnedUIElement);
        }

        if (ElementPatterns.IsPrimitive(CreatedType, out _) || ElementType != null && ElementPatterns.IsPrimitive(ElementType, out _))
            return;

        Type valueType = IsArray ? ElementType! : CreatedType;

        lock (ElementPatterns.TypeInfo)
        {
            NestedType = ElementPatterns.TypeInfo.TryGetValue(valueType, out PatternTypeInfo existingTypeInfo)
                ? existingTypeInfo
                : FindInRootType(rootType, ownerType, valueType);

            if (NestedType != null)
                return;

            NestedType = new PatternTypeInfo(valueType, rootType);
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

    public override string ToString() => Variable.Format(Accessor.ExceptionFormatter);
}
