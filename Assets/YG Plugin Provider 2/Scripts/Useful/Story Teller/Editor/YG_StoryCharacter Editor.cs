using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(YG_StoryCharacter))]
[CanEditMultipleObjects]
public class YG_StoryCharacterEditor : Editor
{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        YG_StoryCharacter character = (YG_StoryCharacter)target;

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        EditorGUILayout.LabelField("Character Preview:", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        GUIStyle boldCenteredLabel = new GUIStyle(EditorStyles.label) {
            fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, fontSize = 14
        };

        if (!character.name.IsEmptyOrWhiteSpaces())
            EditorGUILayout.LabelField(character.name, boldCenteredLabel);
        else
            EditorGUILayout.LabelField("--- NO NAME ---", EditorStyles.centeredGreyMiniLabel);

        EditorGUILayout.Space(5);

        if (character.avatar) {
            Rect previewRect = GUILayoutUtility.GetAspectRect(1.0f);

            Texture2D texture = character.avatar.texture;
            Rect textureRect = character.avatar.textureRect;
            Rect texCoords = new Rect(
                textureRect.x / texture.width,
                textureRect.y / texture.height,
                textureRect.width / texture.width,
                textureRect.height / texture.height
            );

            GUI.DrawTextureWithTexCoords(previewRect, texture, texCoords, true);
        } else
            EditorGUILayout.LabelField("Avatar is null", EditorStyles.centeredGreyMiniLabel);
    }
}