#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace CozyPolyomino
{
    [CustomEditor(typeof(ShapeData))]
    public class ShapeDataEditor : Editor
    {
        private Texture2D _previewTexture;

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height) {
            ShapeData shapeData = (ShapeData)target;
            _previewTexture = shapeData.GetPreviewTexture();
            return _previewTexture;
        }

        public override void OnInspectorGUI() {
            ShapeData data = (ShapeData)target;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawDefaultInspector();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Shape Editor", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            if (data.shapeMap == null || data.shapeMap.Length != data.width * data.height)
                data.shapeMap = new bool[data.width * data.height];

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginVertical();
            for (int y = 0; y < data.height; y++) {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < data.width; x++) {
                    int index = y * data.width + x;
                    bool state = data.shapeMap[index];

                    GUI.backgroundColor = state ? Color.green : Color.white;

                    if (GUILayout.Button("", GUILayout.Width(30), GUILayout.Height(30))) {
                        data.shapeMap[index] = !state;
                        data.InvalidateCache();
                        EditorUtility.SetDirty(data);
                    }

                    GUI.backgroundColor = Color.white;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            
            GUILayout.ExpandWidth(false);
            GUILayout.ExpandHeight(false);

            EditorGUILayout.LabelField("Preview:", EditorStyles.miniBoldLabel);
            
            const float previewSize = 100f;
            Rect previewRect = GUILayoutUtility.GetRect(previewSize, previewSize);
            
            if (!_previewTexture) _previewTexture = data.GetPreviewTexture();
            
            if (_previewTexture) {
                GUI.DrawTexture(
                    new Rect(
                        previewRect.x,
                        previewRect.y,
                        previewSize,
                        previewSize
                    ),
                    _previewTexture,
                    ScaleMode.ScaleToFit
                );
            }

            EditorGUILayout.EndVertical();
        }
    }
}

#endif