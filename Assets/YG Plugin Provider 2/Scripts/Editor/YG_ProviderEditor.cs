using UnityEngine;
using UnityEditor;

public class YG_ProviderEditor : Editor
{
    [MenuItem("YG Plugin Provider/Add Provider AND YandexGame", false, 1)]
    public static void AddAll() {
        AddYandexGame();
        AddProvider();
    }
    
    [MenuItem("YG Plugin Provider/Add Provider", false, 2)]
    public static void AddProvider() {
        if (FindObjectOfType<YG_Provider>(true)) {
            EditorUtility.DisplayDialog("Error",
                "An object with the YG_Provider component already exists on the scene.", "OK"
            );
            return;
        }

        GameObject prefab = Resources.Load<GameObject>("= YG PROVIDER =");

        if (!prefab) {
            EditorUtility.DisplayDialog("Error", "The YG Provider prefab was not found in the Resources folder.", "OK");
            return;
        }

        GameObject provider = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

        if (provider) {
            provider.transform.SetAsFirstSibling();
            provider.transform.position = new Vector3(0f, 0f, 0f);
            provider.transform.eulerAngles = Vector3.zero;
            
            Debug.Log("YG Provider has been successfully added to the scene.");
            
            Undo.RegisterCreatedObjectUndo(provider, "Create YG Provider");
        }
    }
    
    [MenuItem("YG Plugin Provider/Add YandexGame", false, 3)]
    public static void AddYandexGame() {
        if (FindObjectOfType<YG.YandexGame>(true)) {
            EditorUtility.DisplayDialog("Error",
                "An object with the YandexGame component already exists on the scene.", "OK"
            );
            return;
        }
        
        string fileLocation = "Assets/YandexGame/Prefabs/YandexGame.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath(fileLocation, typeof(GameObject)) as GameObject;

        if (!prefab) {
            EditorUtility.DisplayDialog("Error",
                $"The YandexGame prefab was not found in the {fileLocation}", "OK"
            );
            return;
        }
        
        GameObject yandexGame = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

        if (yandexGame) {
            yandexGame.transform.SetAsFirstSibling();
            yandexGame.transform.position = new Vector3(0f, 0f, 0f);
            yandexGame.transform.eulerAngles = Vector3.zero;
            
            Debug.Log("YandexGame has been successfully added to the scene.");
            
            Undo.RegisterCreatedObjectUndo(yandexGame, "Create YandexGame");
        }
    }
    
    [MenuItem("YG Plugin Provider/Add GameMenu (Menu)", false, 4)]
    public static void AddGameMenuMenu() {
        if (FindObjectOfType<YG_GameMenu>(true)) {
            EditorUtility.DisplayDialog("Error",
                "An object with the YG_GameMenu component already exists on the scene.", "OK"
            );
            return;
        }

        GameObject prefab = Resources.Load<GameObject>("= YG GAME MENU (Menu) =");

        if (!prefab) {
            EditorUtility.DisplayDialog("Error", "The YG GameMenu prefab was not found in the Resources folder.", "OK");
            return;
        }

        GameObject provider = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

        if (provider) {
            provider.transform.SetAsFirstSibling();
            provider.transform.position = new Vector3(0f, 0f, 0f);
            provider.transform.eulerAngles = Vector3.zero;
            
            Debug.Log("YG GameMenu has been successfully added to the scene.");
            
            Undo.RegisterCreatedObjectUndo(provider, "Create YG GameMenu");
        }
    }
    
    [MenuItem("YG Plugin Provider/Add GameMenu (Game)", false, 5)]
    public static void AddGameMenuGame() {
        if (FindObjectOfType<YG_GameMenu>(true)) {
            EditorUtility.DisplayDialog("Error",
                "An object with the YG_GameMenu component already exists on the scene.", "OK"
            );
            return;
        }

        GameObject prefab = Resources.Load<GameObject>("= YG GAME MENU (Game) =");

        if (!prefab) {
            EditorUtility.DisplayDialog("Error", "The YG GameMenu prefab was not found in the Resources folder.", "OK");
            return;
        }

        GameObject provider = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

        if (provider) {
            provider.transform.SetAsFirstSibling();
            provider.transform.position = new Vector3(0f, 0f, 0f);
            provider.transform.eulerAngles = Vector3.zero;
            
            Debug.Log("YG GameMenu has been successfully added to the scene.");
            
            Undo.RegisterCreatedObjectUndo(provider, "Create YG GameMenu");
        }
    }
    
    [MenuItem("YG Plugin Provider/Add GameMenu (Own Purposes)", false, 6)]
    public static void AddGameMenuOwn() {
        if (FindObjectOfType<YG_GameMenu>(true)) {
            EditorUtility.DisplayDialog("Error",
                "An object with the YG_GameMenu component already exists on the scene.", "OK"
            );
            return;
        }

        GameObject prefab = Resources.Load<GameObject>("= YG GAME MENU (Own Purposes) =");

        if (!prefab) {
            EditorUtility.DisplayDialog("Error", "The YG GameMenu prefab was not found in the Resources folder.", "OK");
            return;
        }

        GameObject provider = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

        if (provider) {
            provider.transform.SetAsFirstSibling();
            provider.transform.position = new Vector3(0f, 0f, 0f);
            provider.transform.eulerAngles = Vector3.zero;
            
            Debug.Log("YG GameMenu has been successfully added to the scene.");
            
            Undo.RegisterCreatedObjectUndo(provider, "Create YG GameMenu");
        }
    }
}