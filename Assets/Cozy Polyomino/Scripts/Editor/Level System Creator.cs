#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace CozyPolyomino
{
    public static class LevelSystemCreator
    {
        [MenuItem("Cozy Polyomino/Create Level System")]
        public static void CreateLevelSystem() {
            LevelSystemData system = ScriptableObject.CreateInstance<LevelSystemData>();

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Level System",
                "Level System",
                "asset",
                "Choose where to save the Level System"
            );

            if (string.IsNullOrEmpty(path)) return;

            AssetDatabase.CreateAsset(system, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = system;
        }
    }
}

#endif