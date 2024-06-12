using DanielWillett.ReflectionTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Uncreated.Framework.UI.Reflection;

internal static class UIElementDiscovery
{
    public static void LinkAllElements(ILogger logger, UnturnedUI ui, List<UnturnedUIElement> elements)
    {
        int depth = 0;
        DiscoverElements(logger, ui, elements, ref depth, ui.DebugLogging);
    }
    private static void DiscoverElements(ILogger logger, object value, List<UnturnedUIElement> elements, ref int depth, bool debug)
    {
        Type type = value.GetType();

        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        if (depth == 0)
        {
            flags |= BindingFlags.NonPublic;
        }

        FieldInfo[] fields = type.GetFields(flags);
        PropertyInfo[] properties = type.GetProperties(flags);
        for (int i = 0; i < fields.Length; ++i)
        {
            FieldInfo field = fields[i];
            if (!IsIgnored(field, value))
            {
                object val = field.GetValue(value);
                Discover(logger, val, depth, elements, debug);
            }
        }
        for (int i = 0; i < properties.Length; ++i)
        {
            PropertyInfo property = properties[i];
            if (!IsIgnored(property, value) && property.GetGetMethod(true) != null)
            {
                object val = property.GetGetMethod(true).Invoke(value, Array.Empty<object>());
                Discover(logger, val, depth, elements, debug);
            }
        }

        ++depth;
    }

    private static bool IsIgnored(MemberInfo member, object value)
    {
        return member.DeclaringType == typeof(UnturnedUI) || member.IsIgnored() || Attribute.IsDefined(member, typeof(IgnoreIfDefinedTypeAttribute)) && value.GetType() == member.DeclaringType;
    }
    private static void Discover(ILogger logger, object val, int depth, List<UnturnedUIElement> elements, bool debug)
    {
        if (val is UnturnedUIElement elem)
        {
            elements.Add(elem);
            if (debug)
                logger.LogInformation("[{0}] Found element: {1}.", nameof(UIElementDiscovery), elem);
        }
        else if (val is IEnumerable enumerable)
        {
            foreach (object value2 in enumerable)
            {
                if (value2 is UnturnedUIElement elem2)
                {
                    elements.Add(elem2);
                    if (debug)
                        logger.LogInformation("[{0}] Found element (enumerable member): {1}.", nameof(UIElementDiscovery), elem2);
                }
                else if (value2 != null)
                {
                    int depth2 = depth + 1;
                    DiscoverElements(logger, value2, elements, ref depth2, debug);
                    if (debug)
                        logger.LogInformation("[{0}] Found nested type (enumerable member): {1}.", nameof(UIElementDiscovery), value2);
                }
            }
        }
        else if (val != null)
        {
            int depth2 = depth + 1;
            DiscoverElements(logger, val, elements, ref depth2, debug);
            if (debug)
                logger.LogInformation("[{0}] Found nested type: {1}.", nameof(UIElementDiscovery), val);
        }
    }
}