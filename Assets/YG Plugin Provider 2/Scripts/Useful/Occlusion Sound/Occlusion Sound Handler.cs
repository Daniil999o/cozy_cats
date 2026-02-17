using System.Linq;
using UnityEngine;

public static class OcclusionSoundHandler
{
    private static AudioListener _mainListener;

    public static AudioListener MainListener {
        get {
            if (!_mainListener || !_mainListener.isActiveAndEnabled) {
                AudioListener[] listeners =
                    Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

                _mainListener = listeners.FirstOrDefault(x => x.isActiveAndEnabled);
            }

            return _mainListener;
        }
    }
}