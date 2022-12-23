using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
public static class Utility
{
    public static int Clampi(int v, int min, int max)
    {
        return v < min ? min : v > max ? max : v;
    }
    public static T Log<T>(T param, string message = "")
    {
        Debug.Log(message + param.ToString());
        return param;
    }
    public static IEnumerable<Src> Generate<Src>(Src seed, Func<Src, Src> generator)
    {
        while (true)
        {
            yield return seed;
            seed = generator(seed);
        }
    }
    public static bool In<T>(this T x, HashSet<T> set)
    {
        return set.Contains(x);
    }
    public static bool In<K, V>(this KeyValuePair<K, V> x, Dictionary<K, V> dict)
    {
        return dict.Contains(x);
    }
    public static void UpdateWith<K, V>(this Dictionary<K, V> a, Dictionary<K, V> b)
    {
        foreach (var kvp in b) a[kvp.Key] = kvp.Value;
    }
    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> list)
    {
        return new HashSet<T>(list);
    }
    public static V DefaultGet<K, V>(
        this Dictionary<K, V> dict,
        K key,
        Func<V> defaultFactory
    )
    {
        V v;
        if (!dict.TryGetValue(key, out v))
            dict[key] = v = defaultFactory();
        return v;
    }
}