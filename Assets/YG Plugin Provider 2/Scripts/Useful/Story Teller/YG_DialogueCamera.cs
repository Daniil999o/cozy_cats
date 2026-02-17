using UnityEngine;
using System.Collections.Generic;

public class YG_DialogueCamera : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private int currentScene;
    [SerializeField] private List<GameObject> scenes;

    [Space]

    [Header("Scene Cameras")]
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private int currentSceneCamera;
    [SerializeField] private List<SceneCamera> sceneCameras;
    
    [System.Serializable]
    private struct SceneCamera
    {
        public Transform point;
        public RuntimeAnimatorController animatorController;
    }

    private void Start() {
        EnableScene(currentScene);
        EnableSceneCamera(currentSceneCamera);
    }


    public void EnableScene(int index) {
        if (!scenes.InLength(index)) return;
        
        HideScenes();
        currentScene = index;
        scenes[currentScene].SetActive(true);
    }

    public void EnableScene(GameObject scene) {
        if (!scenes.Contains(scene)) return;
        
        EnableScene(scenes.IndexOf(scene));
    }

    public void HideScenes() => scenes.ForEach(x => x.SetActive(false));

    public void EnableSceneCamera(int index) {
        if (!sceneCameras.InLength(index)) return;

        currentSceneCamera = index;
        SceneCamera sceneCamera = sceneCameras[currentSceneCamera];

        mainCamera.transform.CopyTransforms(sceneCamera.point);

        if (sceneCamera.animatorController && mainCamera.TryGetComponent(out Animator animator))
            animator.runtimeAnimatorController = sceneCamera.animatorController;
    }
}