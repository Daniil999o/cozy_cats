using UnityEngine;
using UnityEditor;

public class YG_AnimationPathUpdater : EditorWindow
{
    private string prefix = "Object/";

    [MenuItem("YG Plugin Provider/Animator/Animation Path Updater")]
    public static void ShowWindow() {
        GetWindow<YG_AnimationPathUpdater>("Update Animation Clip Paths");
    }

    private void OnGUI() {
        GUILayout.Label("Update Animation Clip Paths", EditorStyles.boldLabel);
        prefix = EditorGUILayout.TextField("Prefix", prefix);

        if (GUILayout.Button("Update Paths in Selected Clips")) UpdatePathsInSelectedClips();
    }

    private void UpdatePathsInSelectedClips() {
        Object[] selectedObjects = Selection.objects;

        foreach (Object obj in selectedObjects)
            if (obj is AnimationClip clip)
                UpdateClipPaths(clip);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void UpdateClipPaths(AnimationClip clip) {
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
        Debug.Log($"Bindings Count: {bindings.Length}");

        for (int i = 0; i < bindings.Length; i++) {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, bindings[i]);

            AnimationUtility.SetEditorCurve(clip, bindings[i], null);

            Debug.Log($"Removed binding: {bindings[i].path}");

            string newPath = prefix + bindings[i].path;
            bindings[i].path = newPath;

            AnimationUtility.SetEditorCurve(clip, bindings[i], curve);

            Debug.Log($"New Path: {bindings[i].path}");
        }

        Debug.Log($"Updated paths in animation clip: {clip.name}");
    }
}