using System.Collections.Generic;

public static class Utils
{
    public static bool TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, ref TValue value)
    {
        if(dict.ContainsKey(key))
        {
            value = dict[key];
            return true;
        }
        return false;
    }
}
