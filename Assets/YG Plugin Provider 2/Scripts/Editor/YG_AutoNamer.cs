using UnityEditor;
using UnityEngine;

public class YG_AutoNamer : EditorWindow
{
    private string _baseName = "NewName";
    private int _startIndex = 1;
    private NumberFormat _numberFormat = NumberFormat.Parentheses;
    private Vector2 _scrollPos;

    private enum NumberFormat{
        Parentheses, // (1)
        Underscore,  // _1
        Space,       //  1
        Dash,        // -1
        None         // без чисел
    }

    [MenuItem("YG Plugin Provider/Files/Auto Namer")]
    public static void ShowWindow() {
        YG_AutoNamer window = GetWindow<YG_AutoNamer>("YG Auto Namer");
        window.minSize = new Vector2(300, 240);
    }

    private void OnGUI() {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("⚙ Renaming Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        _baseName = EditorGUILayout.TextField(new GUIContent("Base name:"), _baseName);
        _startIndex = EditorGUILayout.IntField(new GUIContent("Initial number:"), _startIndex);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Numbering format:", EditorStyles.boldLabel);

        _numberFormat = (NumberFormat)GUILayout.SelectionGrid(
            (int)_numberFormat,
            new [] {
                "(1)", "_1", " 1", "-1", "without numbers"
            },
            1,
            EditorStyles.radioButton
        );

        EditorGUILayout.Space(10);
        if (GUILayout.Button("🔁 Rename selected", GUILayout.Height(35))) RenameSelectedObjects();

        EditorGUILayout.HelpBox("Supports renaming objects in the Hierarchy and files in the Project.", MessageType.Info
        );
    }

    private void RenameSelectedObjects() {
        Object[] selectedObjects = Selection.objects;

        if (selectedObjects.Length == 0) {
            EditorUtility.DisplayDialog("YG Auto Namer", "No objects or files have been selected!", "OK");
            return;
        }

        Undo.RecordObjects(selectedObjects, "YG Auto Rename");

        int index = _startIndex;
        foreach (Object obj in selectedObjects) {
            string newName = GenerateName(index);

            obj.name = newName;

            string path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path)) AssetDatabase.RenameAsset(path, newName);

            index++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        // EditorUtility.DisplayDialog("YG Auto Namer", $"Renamed objects: {selectedObjects.Length}", "OK");
    }

    private string GenerateName(int index) {
        if (_numberFormat == NumberFormat.None) return _baseName;

        string postfix = index.ToString();

        switch (_numberFormat) {
            case NumberFormat.Parentheses:
                return $"{_baseName} ({postfix})";
            case NumberFormat.Dash:
                return $"{_baseName}-{postfix}";
            case NumberFormat.Underscore:
                return $"{_baseName}_{postfix}";
            case NumberFormat.Space:
                return $"{_baseName} {postfix}";
            default:
                return _baseName;
        }
    }
}