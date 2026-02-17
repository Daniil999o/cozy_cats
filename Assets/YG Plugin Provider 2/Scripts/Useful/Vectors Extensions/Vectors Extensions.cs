using System;
using UnityEngine;
using Random = UnityEngine.Random;

public static class VectorsExtensions
{
    public static Vector2 RandomVector2(this float range) => OneVector2(range.RandomRange());
    
    public static Vector3 RandomVector3(this float range) => OneVector3(range.RandomRange());

    
    public static Vector2 OneVector2(this float value) => new Vector2(value, value);
    
    public static Vector3 OneVector3(this float value) => new Vector3(value, value, value);

    public static Vector3 Optimize(this Vector3 value, int decimals = 2) =>
        new Vector3(value.x.Optimize(decimals), value.y.Optimize(decimals), value.z.Optimize(decimals));
    
    public static Vector2 Optimize(this Vector2 value, int decimals = 2) =>
        new Vector2(value.x.Optimize(decimals), value.y.Optimize(decimals));

    public static float Optimize(this float value, int decimals = 1) => (float)Math.Round((decimal)value, decimals);
    
    
    public static float RandomRange(this float range) => Random.Range(-range, range);
    public static float Randomize(this float range) => RandomRange(range);

    public static Vector3 Randomize(this Vector3 vector) =>
        new Vector3(vector.x.RandomRange(), vector.y.RandomRange(), vector.z.RandomRange());
    
    public static Vector2 Randomize(this Vector2 vector) =>
        new Vector2(vector.x.RandomRange(), vector.y.RandomRange());

    public static float Clamp(this float value, float min, float max) => Mathf.Clamp(value, min, max);
    public static float Clamp(this int value, int min, int max) => Mathf.Clamp(value, min, max);

    public static float Clamp(this float value, Vector2 minMax) => value.Clamp(minMax.x, minMax.y);
    public static float Clamp(this int value, Vector2Int minMax) => value.Clamp(minMax.x, minMax.y);
    
    public static float ClampAngle(this float angle, float min, float max) {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;

        return angle.Clamp(min, max);
    }

    public static float ClampAngle(this float angle, Vector2 minMax) => angle.ClampAngle(minMax.x, minMax.y);

    public static float Clamp01(this float value) => Mathf.Clamp01(value);

    public static Vector3 Multiply(this Vector3 v1, Vector3 v2) =>
        new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);

    public static float Multiply(this float val, Vector3 v) => val * v.magnitude;
}