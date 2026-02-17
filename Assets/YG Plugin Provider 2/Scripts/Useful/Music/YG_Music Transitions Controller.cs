using UnityEngine;

public class YG_MusicTransitionsController : MonoBehaviour
{
    private static YG_MusicTransition _activeController;

    public static YG_MusicTransition ActiveController {
        get {
            if (!_activeController)
                _activeController = FindFirstObjectByType<YG_MusicTransition>(FindObjectsInactive.Include);

            return _activeController;
        }
    }

    public static void NextTransition() {
        if (ActiveController) ActiveController.NextTransition();
    }
}