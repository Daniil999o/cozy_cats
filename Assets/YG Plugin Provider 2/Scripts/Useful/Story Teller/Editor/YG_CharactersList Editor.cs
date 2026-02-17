using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(YG_CharactersList))]
public class YG_CharactersListEditor : Editor
{
    private Vector2 _scrollPosition;

    public override void OnInspectorGUI() {
        serializedObject.Update();
        
        string[] guids = AssetDatabase.FindAssets("t:YG_StoryCharacter");
        YG_StoryCharacter[] characters = new YG_StoryCharacter[guids.Length];
        for (int i = 0; i < guids.Length; i++) {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            characters[i] = AssetDatabase.LoadAssetAtPath<YG_StoryCharacter>(path);
        }
        
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        
        for (int i = 0; i < characters.Length; i++) {
            YG_StoryCharacter character = characters[i];
            if (character) {
                EditorGUILayout.BeginHorizontal();
                
                GUILayout.Box(character.avatar.texture, GUILayout.Width(64), GUILayout.Height(64));

                EditorGUILayout.BeginVertical();
                
                EditorGUILayout.LabelField(character.id);
                
                EditorGUILayout.LabelField(character.name, EditorStyles.boldLabel);
                
                if (GUILayout.Button("Select")) {
                    Selection.activeObject = character;
                    EditorGUIUtility.PingObject(character);
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
            }
        }

        EditorGUILayout.EndScrollView();
        
        serializedObject.ApplyModifiedProperties();
    }
}