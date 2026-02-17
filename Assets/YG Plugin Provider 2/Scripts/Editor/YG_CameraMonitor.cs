using UnityEngine;
using UnityEditor;
using System.Linq;

public class YG_CameraMonitor : EditorWindow
{
    private Camera _selectedCamera;
    private int _selectedCameraIndex = 0;
    private RenderTexture _previewRenderTexture;
    
    private double _timer;
    private const float RefreshRate = 30f;

    [MenuItem("YG Plugin Provider/Camera Monitor")]
    public static void ShowWindow() {
        GetWindow<YG_CameraMonitor>("Camera Monitor");
    }

    private void OnEnable() {
        if (Camera.main != null) _selectedCamera = Camera.main;
    }

    private void OnDisable() {
        if (_previewRenderTexture != null) {
            _previewRenderTexture.Release();
            DestroyImmediate(_previewRenderTexture);
        }
    }

    private void Update() {
        if (_timer > 0) {
            _timer -= Time.deltaTime;
        } else {
            _timer = 1f / RefreshRate;
            Repaint();
        }
    }

    private void OnGUI() {
        Camera[] allCameras = Resources.FindObjectsOfTypeAll<Camera>()
            .Where(c => c.gameObject.scene.IsValid())
            .ToArray();

        if (allCameras.Length == 0) {
            EditorGUILayout.HelpBox("No cameras were found in the scene.", MessageType.Warning);
            return;
        }

        string[] cameraNames = new string[allCameras.Length];
        string[] allCameraNames = new string[allCameras.Length];
        
        for (int i = 0; i < allCameras.Length; i++) {
            string status = allCameras[i].gameObject.activeInHierarchy ? "[Active]" : "[Inactive]";
            string camName = allCameras[i].name;

            int index = 1;
            
            while (allCameraNames.Contains(camName)) camName = allCameras[i].name + $"_{index++}";
            
            allCameraNames[i] = camName;
            cameraNames[i] = $"{camName} {status}";
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Camera Selection", EditorStyles.boldLabel);
        
        if (_selectedCamera == null && allCameras.Length > 0)
            _selectedCamera = Camera.main != null ? Camera.main : allCameras[0];

        if (_selectedCamera != null) {
            _selectedCameraIndex = System.Array.IndexOf(allCameras, _selectedCamera);
            if (_selectedCameraIndex == -1) _selectedCameraIndex = 0;
        }

        int newIndex = EditorGUILayout.Popup("Camera:", _selectedCameraIndex, cameraNames);
        if (newIndex != _selectedCameraIndex || _selectedCamera == null) {
            _selectedCameraIndex = newIndex;
            _selectedCamera = allCameras[_selectedCameraIndex];
        }

        if (_selectedCamera == null) return;

        EditorGUILayout.Space();

        DrawCameraPreview(_selectedCamera);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Camera Options", EditorStyles.boldLabel);

        DrawCameraInfo(_selectedCamera);
    }

    private void DrawCameraPreview(Camera cam) {
        float aspect = cam.aspect;
        float width = position.width - 20;
        float height = width / aspect;

        if (_previewRenderTexture == null || _previewRenderTexture.width != (int)width ||
            _previewRenderTexture.height != (int)height) {
            if (_previewRenderTexture != null) _previewRenderTexture.Release();
            _previewRenderTexture = new RenderTexture((int)width, (int)height, 24);
        }

        RenderTexture oldTarget = cam.targetTexture;
        RenderTexture oldActive = RenderTexture.active;

        cam.targetTexture = _previewRenderTexture;
        cam.Render();

        cam.targetTexture = oldTarget;
        RenderTexture.active = oldActive;

        Rect previewRect = GUILayoutUtility.GetRect(width, height);
        if (Event.current.type == EventType.Repaint)
            GUI.DrawTexture(previewRect, _previewRenderTexture, ScaleMode.ScaleToFit, false);
    }

    private void DrawCameraInfo(Camera cam) {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Object Name:", cam.name);
        EditorGUILayout.LabelField("Status:", cam.gameObject.activeInHierarchy ? "Active" : "Inactive");

        if (cam.orthographic) {
            EditorGUILayout.LabelField("Type:", "Orthographic");
            EditorGUILayout.LabelField("Size:", cam.orthographicSize.ToString());
        } else {
            EditorGUILayout.LabelField("Тип:", "Perspective");
            EditorGUILayout.LabelField("FOV:", cam.fieldOfView.ToString());
        }

        EditorGUILayout.LabelField("Near Clip Plane:", cam.nearClipPlane.ToString());
        EditorGUILayout.LabelField("Far Clip Plane:", cam.farClipPlane.ToString());
        EditorGUILayout.LabelField("Position:", cam.transform.position.ToString());
        EditorGUILayout.LabelField("Rotation:", cam.transform.eulerAngles.ToString());

        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Select in hierarchy")) {
            Selection.activeGameObject = cam.gameObject;
            EditorGUIUtility.PingObject(cam.gameObject);
        }
    }
}