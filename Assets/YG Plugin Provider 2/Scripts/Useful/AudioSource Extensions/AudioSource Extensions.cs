using UnityEngine;
using UnityEngine.Audio;

public static class AudioSourceExtensions
{
    public static Audio3DPreset VeryQuietFxPreset = new Audio3DPreset(3f, 20f);
    public static Audio3DPreset QuietFxPreset = new Audio3DPreset(10f, 70f);
    public static Audio3DPreset NormalFxPreset = new Audio3DPreset(20f, 200f);
    public static Audio3DPreset LoudFxPreset = new Audio3DPreset(100f, 500f);


    public static void PlayFx(this AudioClip[] clips, float volume = 1f) => PlayFx(clips.Choice(), volume);
    
    public static void PlayFx(this AudioClip clip, float volume = 1f) {
        AudioSourceSettings.MainSource.PlayOneShot(clip, volume);
    }


    public static void ShotFx(this AudioClip[] clips,
        Vector3 pos,
        Audio3DPreset preset = null,
        float volume = 1f,
        AudioMixerGroup mixer = null) {
        if (clips.Length <= 0) return;
        
        ShotFx(clips.Choice(), pos, preset, volume, mixer);
    }

    public static void ShotFx(this AudioClip clip, Vector3 pos, Audio3DPreset preset = null, float volume = 1f, AudioMixerGroup mixer = null) {
        if (preset == null) preset = QuietFxPreset;
        if (mixer == null) mixer = AudioSourceSettings.FxMixer;
        
        AudioSource source = new GameObject().AddComponent<AudioSource>();

        source.transform.position = pos;

        source.playOnAwake = false;
        source.loop = false;
        
        source.clip = clip;
        source.volume = volume;

        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = preset.minDistance;
        source.maxDistance = preset.maxDistance;

        source.outputAudioMixerGroup = mixer;
        
        source.Play();
        
        GameObject.Destroy(source.gameObject, clip.length + 0.1f);
    }
}

public class Audio3DPreset
{
    public float minDistance;
    public float maxDistance;

    public Audio3DPreset(float minDistance, float maxDistance) {
        this.minDistance = minDistance;
        this.maxDistance = maxDistance;
    }
}