using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class YG_SceneManagerEditor : EditorWindow
{
    [MenuItem("YG Plugin Provider/SceneManager")]
    public static void ShowWindow() {
        GetWindow<YG_SceneManagerEditor>("YG SceneManager");
    }

    private void OnGUI() {
        // Получаем список всех сцен, добавленных в билд
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

        // Проверяем, есть ли сцены в билде
        if (scenes.Length == 0) {
            EditorGUILayout.HelpBox("Нет сцен в Build Settings!", MessageType.Warning);
            return;
        }

        // Отображаем кнопки для каждой сцены
        for (int i = 0; i < scenes.Length; i++) {
            EditorBuildSettingsScene scene = scenes[i];

            // Проверяем, включена ли сцена в билд
            if (scene.enabled) {
                // Получаем имя сцены
                string sceneName = scene.path.Substring(scene.path.LastIndexOf('/') + 1).Replace(".unity", "");

                // Создаем кнопку с именем сцены
                if (GUILayout.Button(sceneName)) {
                    // Если сцена уже загружена, просто переключаемся на нее
                    if (SceneManager.GetSceneByName(sceneName).isLoaded)
                        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
                    // Иначе загружаем сцену
                    else
                        EditorSceneManager.OpenScene(scene.path);
                }
            }
        }
    }
}