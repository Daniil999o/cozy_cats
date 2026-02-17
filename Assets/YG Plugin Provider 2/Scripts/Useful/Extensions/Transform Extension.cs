using UnityEngine;

public static class TransformExtension
{
    public static void CopyTransforms(this Transform from, Transform to, bool copyScale = false) {
        from.position = to.position;
        from.rotation = to.rotation;

        if (copyScale) from.localScale = to.localScale;
    }
}