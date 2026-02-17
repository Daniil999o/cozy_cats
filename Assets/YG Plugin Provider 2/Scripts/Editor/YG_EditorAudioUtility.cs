using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public static class YG_EditorAudioUtility
{
    private static List<AudioSource> Sources = new List<AudioSource>();

    private static List<AudioSource> GetAudios(AudioClip clip) => Sources.Where(x => x.clip == clip).ToList();

    public static bool IsClipPlaying(AudioClip clip) {
        if (!clip) return false;
        
        UpdateAudioList();
        
        return GetAudios(clip).Any(x => x.isPlaying);
    }

    public static void PlayClip(AudioClip clip, bool pauseAll = true) {
        if (!clip) return;
        
        UpdateAudioList();
        
        if (pauseAll)
            Sources.ForEach(x => x.Pause());
        
        if (GetAudios(clip).Count > 0) {
            while (GetAudios(clip).Count > 1) {
                AudioSource mySource = GetAudios(clip)[0];
                
                GameObject.DestroyImmediate(mySource);
                Sources.Remove(mySource);
            }
            
            GetAudios(clip)[0].Play();
            
            return;
        }
        
        AudioSource source = new GameObject($"TEMP_SOURCE_{clip.name}").AddComponent<AudioSource>();
        source.gameObject.hideFlags = HideFlags.HideInHierarchy;
        
        Sources.Add(source);

        source.clip = clip;
        source.Play();
    }

    public static void StopClip(AudioClip clip) {
        UpdateAudioList();
        
        while (GetAudios(clip).Count > 0) {
            AudioSource source = GetAudios(clip)[0];
            Sources.RemoveAll(x => x == source);
            GameObject.DestroyImmediate(source.gameObject);
        }
    }
    
    public static void PauseClip(AudioClip clip) {
        UpdateAudioList();
        
        GetAudios(clip).ForEach(x => x.Pause());
    }
    
    public static void StopAllClips() {
        UpdateAudioList();
        
        while (Sources.Count > 0) {
            GameObject.DestroyImmediate(Sources[0].gameObject);
            Sources.RemoveAt(0);
        }
    }

    private static void UpdateAudioList() {
        Sources.RemoveAll(x => !x);
    }
}