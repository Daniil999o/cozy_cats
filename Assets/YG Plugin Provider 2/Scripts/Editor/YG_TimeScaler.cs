using UnityEngine;
using UnityEditor;

public class YG_TimeScaler : EditorWindow
{
    private float currentTimeScale;

    [MenuItem("YG Plugin Provider/YG Time Scaler")]
    public static void ShowWindow() {
        GetWindow<YG_TimeScaler>("Time Scaler");
    }

    private void OnEnable() {
        currentTimeScale = Time.timeScale;
    }

    private void OnGUI() {
        GUILayout.Label("Game Time Scale Management", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (Application.isPlaying && Time.timeScale != currentTimeScale) {
            if (GUIUtility.hotControl == 0) currentTimeScale = Time.timeScale;
        }

        float newTimeScale = EditorGUILayout.Slider("Time Scale", currentTimeScale, 0.0f, 50.0f);

        if (newTimeScale != currentTimeScale) {
            currentTimeScale = newTimeScale;
            Time.timeScale = currentTimeScale;
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Pause (0x)")) {
            currentTimeScale = 0f;
            Time.timeScale = currentTimeScale;
        }
        
        if (GUILayout.Button("Snail (0.1x)")) {
            currentTimeScale = 0.1f;
            Time.timeScale = currentTimeScale;
        }
        
        if (GUILayout.Button("Slow Motion (0.5x)")) {
            currentTimeScale = 0.5f;
            Time.timeScale = currentTimeScale;
        }

        if (GUILayout.Button("Normal (1x)")) {
            currentTimeScale = 1f;
            Time.timeScale = currentTimeScale;
        }
        
        if (GUILayout.Button("Faster (2x)")) {
            currentTimeScale = 2f;
            Time.timeScale = currentTimeScale;
        }

        if (GUILayout.Button("Fast (5x)")) {
            currentTimeScale = 5f;
            Time.timeScale = currentTimeScale;
        }
        
        if (GUILayout.Button("Flash (10x)")) {
            currentTimeScale = 10f;
            Time.timeScale = currentTimeScale;
        }
        
        if (GUILayout.Button("Whoosh (20x)")) {
            currentTimeScale = 20f;
            Time.timeScale = currentTimeScale;
        }

        if (!Application.isPlaying) {
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Time changes will only take effect in Play Mode", MessageType.Info);
        }
    }

    private void OnInspectorUpdate() {
        Repaint();
    }
}