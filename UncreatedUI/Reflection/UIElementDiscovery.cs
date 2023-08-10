using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Uncreated.Networking;

namespace Uncreated.Framework.UI.Reflection;

internal static class UIElementDiscovery
{
    public static void LinkAllElements(UnturnedUI ui, List<UnturnedUIElement> elements)
    {
        int depth = 0;
        DiscoverElements(ui, elements, ref depth, ui.DebugLogging);
    }

    private static void DiscoverElements(object value, List<UnturnedUIElement> elements, ref int depth, bool debug)
    {
        Type type = value.GetType();

        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        if (depth == 0)
            flags |= BindingFlags.NonPublic;

        FieldInfo[] fields = type.GetFields(flags);
        PropertyInfo[] properties = type.GetProperties(flags);
        for (int i = 0; i < fields.Length; ++i)
        {
            FieldInfo field = fields[i];
            if (!field.IsIgnored())
            {
                object val = field.GetValue(value);
                Discover(val, depth, elements, debug);
            }
        }
        for (int i = 0; i < properties.Length; ++i)
        {
            PropertyInfo property = properties[i];
            if (!property.IsIgnored() && property.GetGetMethod(true) != null)
            {
                object val = property.GetGetMethod(true).Invoke(value, Array.Empty<object>());
                Discover(val, depth, elements, debug);
            }
        }

        ++depth;
    }

    private static void Discover(object val, int depth, List<UnturnedUIElement> elements, bool debug)
    {
        if (val is UnturnedUIElement elem)
        {
            elements.Add(elem);
            elem.AddIncludedElementsInternal(elements);
            if (debug)
                Logging.LogInfo($"[UI ELEMENT DISCOV] Found element: {elem}.");
        }
        else if (val is IEnumerable enumerable)
        {
            foreach (object value2 in enumerable)
            {
                if (value2 is UnturnedUIElement elem2)
                {
                    elements.Add(elem2);
                    elem2.AddIncludedElementsInternal(elements);
                    if (debug)
                        Logging.LogInfo($"[UI ELEMENT DISCOV] Found element (enumerable member): {elem2}.");
                }
                else if (value2 != null)
                {
                    int depth2 = depth + 1;
                    DiscoverElements(value2, elements, ref depth2, debug);
                    if (debug)
                        Logging.LogInfo($"[UI ELEMENT DISCOV] Found nested type (enumerable member): {value2}.");
                }
            }
        }
        else if (val != null)
        {
            int depth2 = depth + 1;
            DiscoverElements(val, elements, ref depth2, debug);
            if (debug)
                Logging.LogInfo($"[UI ELEMENT DISCOV] Found nested type: {val}.");
        }
    }
}