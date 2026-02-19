using UnityEngine;

public class CatAudioPlayer : MonoBehaviour
{
    [SerializeField] private CatClipsData defaultClips;
    [SerializeField] private CatVoiceData catClips;

    private static bool Dragging = false;
    private bool _dragInThis;

    public void PlayEnter() {
        if (Dragging) return;

        _dragInThis = false;
        
        GameSFX.Instance.ShotClip(defaultClips.enterClips);
    }

    public void PlayExit() {
        if (Dragging || _dragInThis) return;

        GameSFX.Instance.ShotClip(defaultClips.exitClips);
    }

    public void PlayPickup() {
        _dragInThis = true;
        GameSFX.Instance.ShotClip(defaultClips.pickUpClips);
        Dragging = true;
    }

    public void PlayDrop() {
        GameSFX.Instance.ShotClip(defaultClips.dropClips);
        Dragging = false;
    }

    public void PlayMew() => GameSFX.Instance.ShotClip(catClips.normalMewClips, true);
}