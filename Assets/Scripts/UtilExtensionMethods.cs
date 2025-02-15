using System.Collections;
using System.Collections.Generic;

public static class UtilExtensionMethods
{
    public static string ToDebugString(this IEnumerable list, int depth = 1000)
    {
        if (list is string str) {
            return str;
        }

        if (depth == 0)
        {
            return list.ToString();
        }

        List<string> strItems = new();
        foreach (var item in list)
        {
            if (item is IEnumerable enumerable)
            {
                strItems.Add(enumerable.ToDebugString(depth - 1));
            }
            else
            {
                strItems.Add(item.ToString());
            }
        }
        return $"[{string.Join(',', strItems)}]";
    }
}