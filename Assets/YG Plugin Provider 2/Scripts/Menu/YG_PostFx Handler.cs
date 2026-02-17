//
// WARNING!
// Remove comments if you have PostProcessing installed in your project
//

using UnityEngine;
// using UnityEngine.Rendering.PostProcessing;

public class YG_PostFxHandler : MonoBehaviour
{
    private void Start() {
        UpdatePostFx();
    }
    
    private void UpdatePostFx() {
        bool enablePostProcess = YG_GameSettings.Quality == QualitySettings.names.Length - 1;
            
        // foreach (var x in FindObjectsByType<PostProcessLayer>(FindObjectsInactive.Include, FindObjectsSortMode.None
        //          )) {
        //     x.enabled = enablePostProcess;
        // }
        // foreach (var x in FindObjectsByType<PostProcessVolume>(FindObjectsInactive.Include, FindObjectsSortMode.None
        //          )) {
        //     x.enabled = enablePostProcess;
        // }
    }
}