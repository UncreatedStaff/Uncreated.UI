using DanielWillett.ReflectionTools;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Uncreated.Framework.UI.Reflection;

internal static class UIElementDiscovery
{
    internal static void LinkAllElements(UnturnedUI ui, List<UnturnedUIElement> elements)
    {
        int depth = 0;
        DiscoverElements(ui.GetLogger(), ui, elements, ref depth, ui.DebugLogging, ui);
    }

    internal static void DiscoverElements(ILogger? logger, object value, List<UnturnedUIElement> elements, ref int depth, bool debug, UnturnedUI owner, ReflectionCache? cache = null)
    {
        if (value is IEnumerable or IUnturnedUIElementProvider)
        {
            Discover(logger, value, depth, elements, debug, owner);
            return;
        }

        Type type = value.GetType();

        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        if (depth == 0)
        {
            flags |= BindingFlags.NonPublic;
        }

        FieldInfo[] fields;
        PropertyInfo[] properties;
        bool[]? ignoreMask = null;

        if (cache == null || type != cache.Type)
        {
            fields = type.GetFields(flags);
            properties = type.GetProperties(flags);
        }
        else
        {
            if (cache.Fields == null)
            {
                cache.Fields = type.GetFields(flags);
                cache.Properties = type.GetProperties(flags);
                cache.IgnoreMask = new bool[cache.Fields.Length + cache.Properties.Length];
                for (int i = 0; i < cache.IgnoreMask.Length; ++i)
                {
                    MemberInfo member = i >= cache.Fields.Length
                        ? cache.Properties[i - cache.Fields.Length]
                        : cache.Fields[i];

                    bool isIgnored = IsIgnored(member, value);

                    if (!isIgnored && member is PropertyInfo prop)
                        isIgnored &= prop.CanRead;

                    cache.IgnoreMask[i] = isIgnored;
                }
            }

            fields = cache.Fields;
            properties = cache.Properties;
            ignoreMask = cache.IgnoreMask;
        }

        for (int i = 0; i < fields.Length; ++i)
        {
            FieldInfo field = fields[i];
            if (!(ignoreMask == null ? IsIgnored(field, value) : ignoreMask[i]))
            {
                object val = field.GetValue(value);
                Discover(logger, val, depth, elements, debug, owner);
            }
        }
        for (int i = 0; i < properties.Length; ++i)
        {
            PropertyInfo property = properties[i];
            if (!(ignoreMask == null ? IsIgnored(property, value) || !property.CanRead : ignoreMask[i]))
            {
                object val = property.GetValue(value, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, null, null);
                Discover(logger, val, depth, elements, debug, owner);
            }
        }

        ++depth;
    }

    private static bool IsIgnored(MemberInfo member, object value)
    {
        return member.DeclaringType == typeof(UnturnedUI) || member.IsIgnored() || Attribute.IsDefined(member, typeof(IgnoreIfDefinedTypeAttribute)) && value.GetType() == member.DeclaringType || member.IsDefinedSafe<CompilerGeneratedAttribute>();
    }

    private static void Discover(ILogger? logger, object val, int depth, List<UnturnedUIElement> elements, bool debug, UnturnedUI owner)
    {
        if (val is UnturnedUIElement elem)
        {
            if (elements.Contains(elem))
                return;
            elements.Add(elem);
            elem.RegisterOwnerIntl(owner);
            if (debug)
                logger.LogInformation(Properties.Resources.Log_FoundPrimitive, nameof(UIElementDiscovery), elem);
        }
        else if (val is IEnumerable enumerable)
        {
            ReflectionCache? cache = null;
            foreach (object value2 in enumerable)
            {
                if (value2 is UnturnedUIElement elem2)
                {
                    if (elements.Contains(elem2))
                        return;
                    elements.Add(elem2);
                    elem2.RegisterOwnerIntl(owner);
                    if (debug)
                        logger!.LogInformation(Properties.Resources.Log_FoundPrimitiveInEnumerable, nameof(UIElementDiscovery), elem2);
                }
                else if (value2 != null)
                {
                    // adds a cache within enumerables, which will usually always contain the same type
                    Type valueType = value2.GetType();
                    if (cache == null || cache.Type != valueType)
                        cache = new ReflectionCache { Type = value2.GetType() };
                    int depth2 = depth + 1;
                    int oldCt = elements.Count;
                    DiscoverElements(logger, value2, elements, ref depth2, debug, owner, cache);
                    if (debug && elements.Count != oldCt)
                        logger!.LogInformation(Properties.Resources.Log_FoundOtherInEnumerable, nameof(UIElementDiscovery), elements.Count - oldCt, Accessor.Formatter.Format(value2.GetType()));
                }
            }
        }
        else if (val is IUnturnedUIElementProvider provider)
        {
            int ct = 0;
            foreach (UnturnedUIElement element in provider.EnumerateElements())
            {
                ++ct;
                if (elements.Contains(element))
                    return;
                elements.Add(element);
                element.RegisterOwnerIntl(owner);
                if (debug)
                    logger!.LogInformation(Properties.Resources.Log_FoundPrimitiveInIUnturnedUIElementProvider, nameof(UIElementDiscovery), element);
            }

            if (debug && ct != 0)
                logger!.LogInformation(Properties.Resources.Log_FoundIUnturnedUIElementProvider, nameof(UIElementDiscovery), ct);
        }
        else if (val != null)
        {
            // can not nest UIs
            if (val is UnturnedUI)
                return;

            int depth2 = depth + 1;
            int oldCt = elements.Count;
            DiscoverElements(logger, val, elements, ref depth2, debug, owner);
            if (debug && elements.Count != oldCt)
                logger!.LogInformation(Properties.Resources.Log_FoundOther, nameof(UIElementDiscovery), elements.Count - oldCt, Accessor.Formatter.Format(val.GetType()));
        }
    }

    public class ReflectionCache
    {
#nullable disable
        public Type Type;
        public FieldInfo[] Fields;
        public PropertyInfo[] Properties;
        public bool[] IgnoreMask;
#nullable restore
    }
}