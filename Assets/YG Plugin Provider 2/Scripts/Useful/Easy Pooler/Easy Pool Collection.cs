using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class EasyPoolCollection<T>
{
    public string Name { get; set; } = "Unnamed";
    public int Size { get; set; }
    
    public List<Object> PooledObjects { get; set; }

    private T _myType;

    public EasyPoolCollection(string name, T myType, int size = 10) {
        _myType = myType;
        Name = name;
        Size = size;
    }

    public void PoolObject(T pooled) {
        try {
            Object obj = pooled as Object;

            if (!obj) return;

            PooledObjects.Add(obj);
        } catch (Exception e) {
            Debug.LogError($"[EasyPoolCollection: {Name}] {e.Message}");
        }
    }

    public void Clear() {
        try {
            PooledObjects.ForEach(Object.DestroyImmediate);
            PooledObjects.Clear();
        } catch (Exception e) {
            Debug.LogError($"[EasyPoolCollection: {Name}] {e.Message}");
        }
    }
}