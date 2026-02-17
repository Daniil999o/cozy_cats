using System.IO;
using UnityEngine;
using UnityEditor;

public class YG_ImageViewer : EditorWindow
{
    private Texture2D _loadedImage;
    private Vector2 _scrollPosition;
    private float _zoom = 1f;

    [MenuItem("YG Plugin Provider/Tools/Image Viewer")]
    public static void ShowWindow() {
        GetWindow<YG_ImageViewer>("Image Viewer");
    }

    private void OnGUI() {
        if (GUILayout.Button("Load image")) {
            string path = EditorUtility.OpenFilePanel("Select Image", "", "png,jpg,jpeg");

            if (path.Length != 0)
                LoadImage(path);
        }

        if (_loadedImage) {
            Event e = Event.current;
            
            if (e.type == EventType.ScrollWheel) {
                Vector2 mousePos = e.mousePosition;
                
                float zoomDelta = -e.delta.y * 0.1f;
                float previousZoom = _zoom;
                
                _zoom += zoomDelta;
                _zoom = Mathf.Clamp(_zoom, 0.1f, 10f);
                
                Vector2 zoomCenter = mousePos + _scrollPosition;
                _scrollPosition = zoomCenter * (_zoom / previousZoom) - mousePos;

                e.Use();
                Repaint();
            }
            
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("-"))
                 Zoom(-0.1f);
            if (GUILayout.Button("+"))
                Zoom(0.1f);
            
            GUILayout.EndHorizontal();

            float aspectRatio = (float)_loadedImage.width / _loadedImage.height;
            float maxWidth = position.width - 20;
            float maxHeight = position.height - 60;

            float displayWidth = _loadedImage.width * _zoom;
            float displayHeight = displayWidth / aspectRatio;

            if (displayHeight > maxHeight) {
                displayHeight = maxHeight * _zoom;
                displayWidth = displayHeight * aspectRatio;
            }
            
            Rect displayRect = new Rect(
                (maxWidth - displayWidth) / 2,
                (maxHeight - displayHeight) / 2,
                displayWidth,
                displayHeight
            );
            
            if (displayWidth > maxWidth || displayHeight > maxHeight) {
                displayRect = new Rect(0, 0, displayWidth, displayHeight);
                _scrollPosition = GUI.BeginScrollView(new Rect(0, 60, position.width, position.height - 60),
                    _scrollPosition, displayRect
                );
                GUI.DrawTexture(displayRect, _loadedImage);
                GUI.EndScrollView();
            } else {
                GUI.DrawTexture(new Rect(displayRect.x, 60 + displayRect.y, displayRect.width, displayRect.height),
                    _loadedImage
                );
            }
        } else {
            EditorGUILayout.LabelField("Image has not been uploaded");
        }
    }

    private void Zoom(float zoomDelta) {
        float previousZoom = _zoom;
        
        _zoom += zoomDelta;
        _zoom = Mathf.Clamp(_zoom, 0.1f, 10f);

        Vector2 zoomCenter = new Vector2(position.width / 2f, (position.height - 60) / 2f) + _scrollPosition;
        _scrollPosition = zoomCenter * (_zoom / previousZoom) -
                          new Vector2(position.width / 2f, (position.height - 60) / 2f);

        Repaint();
    }

    private void LoadImage(string path) {
        byte[] imageData = File.ReadAllBytes(path);

        _loadedImage = new Texture2D(2, 2);
        _loadedImage.LoadImage(imageData);

        _zoom = 1f;
        _scrollPosition = Vector2.zero;
    }
}