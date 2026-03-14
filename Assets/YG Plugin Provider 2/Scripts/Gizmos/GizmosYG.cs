#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

public static class GizmosYG
{
    public static void DrawText(string text, Vector3 worldPosition, Vector2? anchor = null, float textSize = 15f) {
#if UNITY_EDITOR
        SceneView view = SceneView.currentDrawingSceneView;
        if (!view) return;
        Vector3 screenPosition = view.camera.WorldToScreenPoint(worldPosition);
        if (screenPosition.y < 0 || screenPosition.y > view.camera.pixelHeight || screenPosition.x < 0 ||
            screenPosition.x > view.camera.pixelWidth || screenPosition.z < 0)
            return;
        float pixelRatio = HandleUtility.GUIPointToScreenPixelCoordinate(Vector2.right).x -
                           HandleUtility.GUIPointToScreenPixelCoordinate(Vector2.zero).x;
        Handles.BeginGUI();
        GUIStyle style = new GUIStyle(GUI.skin.label) {
            fontSize = (int)textSize,
            normal = new GUIStyleState {
                textColor = Gizmos.color
            }
        };
        if (anchor == null) anchor = Vector2.zero;
        Vector2 size = style.CalcSize(new GUIContent(text)) * pixelRatio;
        Vector2 alignedPosition =
            ((Vector2)screenPosition + size * (((Vector2)anchor + Vector2.left + Vector2.up) / 2f)) *
            (Vector2.right + Vector2.down) + Vector2.up * view.camera.pixelHeight;
        GUI.Label(new Rect(alignedPosition / pixelRatio, size / pixelRatio), text, style);
        Handles.EndGUI();
#endif
    }

    // =========================================================
    // WIREFRAME METHODS
    // =========================================================

    public static void DrawWireCylinder(float radius, float height) {
#if UNITY_EDITOR
        PrepareHandles();

        float halfHeight = height * 0.5f;
        Vector3 top = Vector3.up * halfHeight;
        Vector3 bottom = Vector3.down * halfHeight;

        Handles.DrawWireDisc(top, Vector3.up, radius);
        Handles.DrawWireDisc(bottom, Vector3.up, radius);

        Handles.DrawLine(top + Vector3.forward * radius, bottom + Vector3.forward * radius);
        Handles.DrawLine(top + Vector3.back * radius, bottom + Vector3.back * radius);
        Handles.DrawLine(top + Vector3.right * radius, bottom + Vector3.right * radius);
        Handles.DrawLine(top + Vector3.left * radius, bottom + Vector3.left * radius);

        RestoreHandles();
#endif
    }

    public static void DrawWireCapsule(float radius, float height) {
#if UNITY_EDITOR
        PrepareHandles();

        float capsuleHeight = Mathf.Max(radius * 2, height);
        float focalPoint = capsuleHeight / 2f - radius;

        Vector3 topPoint = Vector3.up * focalPoint;
        Vector3 bottomPoint = Vector3.down * focalPoint;

        Handles.DrawWireDisc(topPoint, Vector3.up, radius);
        Handles.DrawWireDisc(bottomPoint, Vector3.up, radius);

        Handles.DrawLine(topPoint + Vector3.right * radius, bottomPoint + Vector3.right * radius);
        Handles.DrawLine(topPoint + Vector3.left * radius, bottomPoint + Vector3.left * radius);
        Handles.DrawLine(topPoint + Vector3.forward * radius, bottomPoint + Vector3.forward * radius);
        Handles.DrawLine(topPoint + Vector3.back * radius, bottomPoint + Vector3.back * radius);

        Handles.DrawWireArc(topPoint, Vector3.left, Vector3.forward * radius, 180, radius);
        Handles.DrawWireArc(topPoint, Vector3.forward, Vector3.right * radius, 180, radius);
        Handles.DrawWireArc(bottomPoint, Vector3.left, Vector3.back * radius, 180, radius);
        Handles.DrawWireArc(bottomPoint, Vector3.forward, Vector3.left * radius, 180, radius);

        RestoreHandles();
#endif
    }

    // =========================================================
    // SOLID METHODS
    // =========================================================

    public static void DrawCylinder(float radius, float height) {
#if UNITY_EDITOR
        PrepareHandles();

        float halfHeight = height * 0.5f;

        Handles.DrawSolidDisc(Vector3.up * halfHeight, Vector3.up, radius);
        Handles.DrawSolidDisc(Vector3.down * halfHeight, Vector3.up, radius);

        Matrix4x4 bodyMatrix = Handles.matrix * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0),
            new Vector3(radius * 2, radius * 2, height)
        );

        using (new Handles.DrawingScope(bodyMatrix))
            Handles.CylinderHandleCap(0, Vector3.zero, Quaternion.identity, 1f, EventType.Repaint);

        RestoreHandles();
#endif
    }

    public static void DrawCapsule(float radius, float height) {
#if UNITY_EDITOR
        PrepareHandles();

        float capsuleHeight = Mathf.Max(radius * 2, height);
        float cylinderHeight = capsuleHeight - radius * 2;
        float focalPoint = cylinderHeight / 2f;

        Matrix4x4 topMtx = Handles.matrix *
                           Matrix4x4.TRS(Vector3.up * focalPoint, Quaternion.identity, Vector3.one * (radius * 2));
        using (new Handles.DrawingScope(topMtx))
            Handles.SphereHandleCap(0, Vector3.zero, Quaternion.identity, 1f, EventType.Repaint);

        Matrix4x4 botMtx = Handles.matrix *
                           Matrix4x4.TRS(Vector3.down * focalPoint, Quaternion.identity, Vector3.one * (radius * 2));
        using (new Handles.DrawingScope(botMtx))
            Handles.SphereHandleCap(0, Vector3.zero, Quaternion.identity, 1f, EventType.Repaint);

        if (cylinderHeight > 0) {
            Matrix4x4 cylMtx = Handles.matrix * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0),
                new Vector3(radius * 2, radius * 2, cylinderHeight)
            );
            using (new Handles.DrawingScope(cylMtx))
                Handles.CylinderHandleCap(0, Vector3.zero, Quaternion.identity, 1f, EventType.Repaint);
        }

        RestoreHandles();
#endif
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private static Matrix4x4 _prevMatrix;
    private static Color _prevColor;

    private static void PrepareHandles() {
        #if UNITY_EDITOR
        
        _prevMatrix = Handles.matrix;
        _prevColor = Handles.color;
        Handles.matrix = Gizmos.matrix;
        Handles.color = Gizmos.color;
        
        #endif
    }

    private static void RestoreHandles() {
        #if UNITY_EDITOR
        
        Handles.matrix = _prevMatrix;
        Handles.color = _prevColor;
        
        #endif
    }
}