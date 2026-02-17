using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public class YG_AnimatorEditor : EditorWindow
{
    [SerializeField] private GameObject target;
    
    [SerializeField] private AnimationClip clip;
    [SerializeField] private int frameNumber;


    [MenuItem("YG Plugin Provider/Animator/Animator Editor")]
    public static void ShowWindow() {
        YG_AnimatorEditor animatorEditor = GetWindow<YG_AnimatorEditor>("Animator Editor");
        animatorEditor.target = Selection.activeGameObject;
    }

    private void OnGUI() {
        target = (GameObject)EditorGUILayout.ObjectField("Target", target, typeof(GameObject), true);

        if (!target || !target.GetComponent<Animator>()) {
            EditorGUILayout.HelpBox("The object does not have an Animator component", MessageType.Error);
            return;
        }
        
        GUILayout.Space(20);
        
        clip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", clip, typeof(AnimationClip), true);

        if (!clip) return;
        
        int totalFrames = (int)Mathf.Abs(Mathf.Floor(clip.frameRate * clip.length));
        frameNumber = EditorGUILayout.IntSlider("Frame Number", frameNumber, 0, totalFrames);
        
        GUILayout.Space(5);
        
        if (GUILayout.Button("Apply Frame") && clip && target)
            ApplyFrame();
    }

    private void OnSelectionChange() {
        target = Selection.activeGameObject;
        
        Repaint();
    }

    private void ApplyFrame() {
        if (!target || !clip) return;
        
        Undo.RecordObject(target, "Apply Animation Frame");
        
        float time = frameNumber / clip.frameRate;
        
        clip.SampleAnimation(target, time);
        
        EditorSceneManager.MarkSceneDirty(target.scene);
    }
}