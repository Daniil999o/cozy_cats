using System.IO;
using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class YG_ScreenCapture : EditorWindow
{
    private string _lastFolder;
    
    [MenuItem("YG Plugin Provider/Tools/Screen Capture")]
    public static void ShowWindow() {
        GetWindow<YG_ScreenCapture>("Screen Capture");
    }

    private void OnGUI() {
        if (GUILayout.Button("Screenshot Game"))
            TakeGameViewScreenshot();
        if (GUILayout.Button("Screenshot Scene"))
            TakeSceneViewScreenshot();
        
        if (string.IsNullOrEmpty(_lastFolder)) return;
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Open Folder"))
            OpenLastFolder();
    }

    private void TakeGameViewScreenshot() {
        string path = EditorUtility.SaveFilePanel("Save Game screenshot", "", "game_screenshot", "png");
        
        if (path.Length != 0) {
            ScreenCapture.CaptureScreenshot(path);

            _lastFolder = path;
            Debug.Log("Game screenshot saved: " + path);
        }
    }

    private void TakeSceneViewScreenshot() {
        string path = EditorUtility.SaveFilePanel("Save Scene screenshot", "", "scene_screenshot", "png");
        
        if (path.Length == 0) return;
        
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (!sceneView) {
            Debug.LogError("Scene View was not found!");
            return;
        }
        
        Camera camera = sceneView.camera;
        if (!camera) {
            Debug.LogError("Scene View camera was not found!");
            return;
        }
        
        RenderTexture rt = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 24);
        camera.targetTexture = rt;
        camera.Render();
        
        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D(camera.pixelWidth, camera.pixelHeight, TextureFormat.RGB24, false);
        screenShot.ReadPixels(new Rect(0, 0, camera.pixelWidth, camera.pixelHeight), 0, 0);
        screenShot.Apply();
        
        camera.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);
        
        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(path, bytes);

        _lastFolder = path;
        Debug.Log("Scene screenshot saved: " + path);
    }

    private void OpenLastFolder() {
        _lastFolder = Path.GetDirectoryName(_lastFolder);
        
        if (!Directory.Exists(_lastFolder)) {
            Debug.LogError("Folder not found: " + _lastFolder);
            return;
        }
        
        Process.Start("explorer.exe", _lastFolder);
    }
}