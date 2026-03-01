using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public static class ListExtensions
{
    public static List<T> Shuffle<T>(this List<T> list) {
        int n = list.Count;
        
        while (n > 1) {
            n--;
            int k = Random.Range(0, n + 1);
            
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

        return list;
    }

    public static T[] Shuffle<T>(this T[] array) => array.ToList().Shuffle().ToArray();
    
    public static HashSet<T> Shuffle<T>(this HashSet<T> hashSet) => hashSet.ToList().Shuffle().ToHashSet();

    
    public static T Choice<T>(this List<T> list) => list[Random.Range(0, list.Count)];
    
    public static T Choice<T>(this T[] array) => array.ToList().Choice();
    

    public static T Choice<T>(this HashSet<T> hashSet) => hashSet.ToList().Choice();

    
    public static bool IsNull<T>(this List<T> list) => list == null || list.Count <= 0;
    
    public static bool IsNull<T>(this T[] array) => array == null || array.ToList().IsNull();
    

    public static bool IsNull<T>(this HashSet<T> hashSet) => hashSet == null || hashSet.ToList().IsNull();
    

    public static bool InLength<T>(this List<T> list, int id) => id >= 0 && id < list.Count;
    
    public static bool InLength<T>(this T[] array, int id) => array.ToList().InLength(id);
    
    public static bool InLength<T>(this HashSet<T> hashSet, int id) => hashSet.ToList().InLength(id);
    public static bool InLength(this string text, int id) => id >= 0 && id < text.Length;


    public static T GetElement<T>(this List<T> list, int id) => !list.IsNull() && list.InLength(id) ? list[id] : default;
    
    public static T GetElement<T>(this T[] array, int id) => array.ToList().GetElement(id);
    
    public static T GetElement<T>(this HashSet<T> hashSet, int id) => hashSet.ToList().GetElement(id);

    public static T[] SetToFirstEmpty<T>(this T[] array, T value) {
        for (int i = 0; i < array.Length; i++) {
            if (array[i] != null) continue;
            
            array[i] = value;
            break;
        }

        return array;
    }

    public static List<T> SetToFirstEmpty<T>(this List<T> list, T value) =>
        SetToFirstEmpty(list.ToArray(), value).ToList();
    
    
    public static bool SetToFirstEmpty<T>(this T[] array, T value, out T[] result) {
        bool set = false;
        
        for (int i = 0; i < array.Length; i++) {
            if (array[i] != null) continue;
            
            array[i] = value;
            set = true;
            break;
        }

        result = array;

        return set;
    }

    public static bool SetToFirstEmpty<T>(this List<T> list, T value, out List<T> result) {
        bool set = SetToFirstEmpty(list.ToArray(), value, out T[] arrayResult);

        result = arrayResult.ToList();

        return set;
    }
}