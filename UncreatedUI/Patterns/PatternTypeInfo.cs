﻿using DanielWillett.ReflectionTools;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Uncreated.Framework.UI.Patterns;
internal class PatternTypeInfo
{
    public readonly Type Type;
    public readonly PatternVariableInfo[] Variables;
    public PatternTypeInfo(Type type, PatternTypeInfo? rootTypeInfo)
    {
        Type = type;
        Variables = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                        .Concat<MemberInfo>(type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
                        .Where(member => !member.IsIgnored() && member.DeclaringType != typeof(object) && member.DeclaringType != typeof(ValueType) && !member.IsDefinedSafe<CompilerGeneratedAttribute>())
                        .Select(member => new PatternVariableInfo(Accessor.Active.AsVariable(member), rootTypeInfo, this))
                        .ToArray();

        int ct = Variables.Count(x => x.IsRoot);
        if (ct == 0)
            return;

        PatternVariableInfo root = Array.Find(Variables, x => x.IsRoot)!;
        if (ct > 1)
            throw new NotSupportedException($"There can only be one root in a pattern object (like in {root}).");

        foreach (PatternVariableInfo info in Variables)
        {
            if (info is { IsRoot: false, UnderRoot: true })
                info.RootInfo = root;
        }
    }
}
