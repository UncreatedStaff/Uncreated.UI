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
        DiscoverElements(ui.Factory, ui.Logger, ui, elements, ref depth, ui.DebugLogging, ui);
    }

    internal static void DiscoverElements(ILoggerFactory? loggerFactory, ILogger? logger, object value, List<UnturnedUIElement> elements, ref int depth, bool debug, UnturnedUI owner, ReflectionCache? cache = null)
    {
        if (value is IEnumerable)
        {
            Discover(loggerFactory, logger, value, depth, elements, debug, owner);
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
                Discover(loggerFactory, logger, val, depth, elements, debug, owner);
            }
        }
        for (int i = 0; i < properties.Length; ++i)
        {
            PropertyInfo property = properties[i];
            if (!(ignoreMask == null ? IsIgnored(property, value) || !property.CanRead : ignoreMask[i]))
            {
                object val = property.GetValue(value, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, null, null);
                Discover(loggerFactory, logger, val, depth, elements, debug, owner);
            }
        }

        ++depth;
    }

    private static bool IsIgnored(MemberInfo member, object value)
    {
        return member.DeclaringType == typeof(UnturnedUI) || member.IsIgnored() || Attribute.IsDefined(member, typeof(IgnoreIfDefinedTypeAttribute)) && value.GetType() == member.DeclaringType || member.IsDefinedSafe<CompilerGeneratedAttribute>();
    }

    private static void Discover(ILoggerFactory? loggerFactory, ILogger? logger, object val, int depth, List<UnturnedUIElement> elements, bool debug, UnturnedUI owner)
    {
        if (val is UnturnedUIElement elem)
        {
            if (elements.Contains(elem))
                return;
            elements.Add(elem);
            elem.RegisterOwnerIntl(owner, loggerFactory);
            if (debug)
                logger.LogInformation("[{0}] Found element: {1}.", nameof(UIElementDiscovery), elem);
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
                    elem2.RegisterOwnerIntl(owner, loggerFactory);
                    if (debug)
                        logger!.LogInformation("[{0}] Found element (enumerable member): {1}.", nameof(UIElementDiscovery), elem2);
                }
                else if (value2 != null)
                {
                    // adds a cache within enumerables, which will usually always contain the same type
                    cache ??= new ReflectionCache { Type = value2.GetType() };
                    int depth2 = depth + 1;
                    DiscoverElements(loggerFactory, logger, value2, elements, ref depth2, debug, owner, cache);
                    if (debug)
                        logger!.LogInformation("[{0}] Found nested type (enumerable member): {1}.", nameof(UIElementDiscovery), value2);
                }
            }
        }
        else if (val != null)
        {
            // can not nest UIs
            if (val is UnturnedUI)
                return;

            int depth2 = depth + 1;
            DiscoverElements(loggerFactory, logger, val, elements, ref depth2, debug, owner);
            if (debug)
                logger!.LogInformation("[{0}] Found nested type: {1}.", nameof(UIElementDiscovery), val);
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