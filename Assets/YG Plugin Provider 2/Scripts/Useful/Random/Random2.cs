using System;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Random2
{
    public static bool Chance(float chance) => Random.Range(0f, 1f) <= chance;
    
    public static float Range(float min, float max) => Random.Range(min, max);
    
    public static float Range(Vector2 minMax) => Random.Range(minMax.x, minMax.y);
    
    public static float Range01() => Random.Range(0f, 1f);

    public static float RangeFrom0(float range) => Random.Range(0f, range);
    
    public static float Range(float range) => Random.Range(-range, range);

    public static Vector3 Vector3(Vector3 ranges) => new Vector3(Range(ranges.x), Range(ranges.y), Range(ranges.z));

    public static Vector2 Vector2(Vector2 ranges) => new Vector2(Range(ranges.x), Range(ranges.y));

    public static Vector3 Vector3(Vector3 min, Vector3 max) =>
        new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));

    public static Vector2 Vector2(Vector2 min, Vector2 max) =>
        new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
}