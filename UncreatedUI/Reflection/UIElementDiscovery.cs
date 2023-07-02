using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Uncreated.Framework.UI.Reflection;

internal static class UIElementDiscovery
{
    public static void LinkAllElements(UnturnedUI ui, List<UnturnedUIElement> element)
    {
        List<Type> recursiveChecker = new List<Type>();
        int depth = 0;
        DiscoverElements(ui, element, recursiveChecker, ref depth);
    }

    private static void DiscoverElements(object value, List<UnturnedUIElement> elements, List<Type> recursiveChecker, ref int depth)
    {
        Type type = value.GetType();
        if (depth != 0 && recursiveChecker.Contains(type))
            return;
        recursiveChecker.Add(type);

        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        if (depth == 0)
            flags |= BindingFlags.NonPublic;

        FieldInfo[] fields = type.GetFields(flags);
        PropertyInfo[] properties = type.GetProperties(flags);
        for (int i = 0; i < fields.Length; ++i)
        {
            FieldInfo field = fields[i];
            if (!field.IsIgnored() && (CanCheckField(field.FieldType) || depth == 0))
            {
                object val = field.GetValue(value);
                Discover(val, depth, elements, recursiveChecker);
            }
        }
        for (int i = 0; i < properties.Length; ++i)
        {
            PropertyInfo property = properties[i];
            if (!property.IsIgnored() && property.GetGetMethod(true) != null && (CanCheckField(property.PropertyType) || depth == 0))
            {
                object val = property.GetGetMethod(true).Invoke(value, Array.Empty<object>());
                Discover(val, depth, elements, recursiveChecker);
            }
        }

        ++depth;
    }

    private static void Discover(object val, int depth, List<UnturnedUIElement> elements, List<Type> recursiveChecker)
    {
        if (val is UnturnedUIElement elem)
        {
            elements.Add(elem);
            elem.AddIncludedElementsInternal(elements);
        }
        else if (val is IEnumerable enumerable)
        {
            foreach (object value2 in enumerable)
            {
                int depth2 = depth + 1;
                DiscoverElements(value2, elements, recursiveChecker.ToList(), ref depth2);
            }
        }
        else
        {
            int depth2 = depth + 1;
            DiscoverElements(val, elements, recursiveChecker, ref depth2);
        }
    }

    private static bool CanCheckField(Type type)
    {
        return typeof(UnturnedUIElement).IsAssignableFrom(type) || typeof(IEnumerable<UnturnedUIElement>).IsAssignableFrom(type);
    }
}