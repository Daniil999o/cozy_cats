using UnityEngine;

public class CatAudioPlayer : MonoBehaviour
{
    [SerializeField] private CatClipsData defaultClips;

    public void PlayEnter() => GameSFX.Instance.ShotClip(defaultClips.enterClips);
    public void PlayExit() => GameSFX.Instance.ShotClip(defaultClips.exitClips);
    
    public void PlayPickup() => GameSFX.Instance.ShotClip(defaultClips.pickUpClips);
    public void PlayDrop() => GameSFX.Instance.ShotClip(defaultClips.dropClips);
}