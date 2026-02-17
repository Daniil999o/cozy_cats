using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Music Playlist", menuName = "YG Plugin Provider/Music Playlist")]
public class YG_MusicPlaylist : ScriptableObject
{
    public bool autoPlay = true;
    public List<AudioClip> tracks = new List<AudioClip>();
}

public class YG_MusicPlaylistEditor : EditorWindow
{
    private YG_MusicPlaylist playlist;
    private int currentTrackIndex;
    private AudioSource audioSource;
    private bool isPlaying;

    private const string PlaylistAsset = "Assets/YG Plugin Provider 2/Music Playlist/MusicPlaylist.asset";

    [MenuItem("YG Plugin Provider/Music/Window")]
    public static void ShowWindow() {
        GetWindow<YG_MusicPlaylistEditor>("Music Playlist");
    }

    [MenuItem("YG Plugin Provider/Music/Play Pause")]
    public static void PlayPause() {
        YG_MusicPlaylistEditor window = GetWindow<YG_MusicPlaylistEditor>();
        window.PlayPauseToggle();
    }

    [MenuItem("YG Plugin Provider/Music/Volume +10")]
    public static void VolumeUp() {
        YG_MusicPlaylistEditor window = GetWindow<YG_MusicPlaylistEditor>();
        window.AdjustVolume(0.1f);
    }

    [MenuItem("YG Plugin Provider/Music/Volume -10")]
    public static void VolumeDown() {
        YG_MusicPlaylistEditor window = GetWindow<YG_MusicPlaylistEditor>();
        window.AdjustVolume(-0.1f);
    }

    [MenuItem("YG Plugin Provider/Music/Playlist")]
    public static void OpenPlaylist() {
        Selection.activeObject =
            AssetDatabase.LoadAssetAtPath<YG_MusicPlaylist>(PlaylistAsset);
    }

    private const string PlayerName = "= YG MUSIC PLAYER =";

    private static AudioSource _audioSource;
    private static AudioSource AudioSource {
        get {
            if (GameObject.Find(PlayerName))
                _audioSource = GameObject.Find(PlayerName).GetComponent<AudioSource>();
            else
                _audioSource = null;
            
            if (!_audioSource) {
                GameObject audioSourceObject = new GameObject(PlayerName);
                audioSourceObject.hideFlags = HideFlags.HideInHierarchy;

                _audioSource = audioSourceObject.AddComponent<AudioSource>();
            
                _audioSource.playOnAwake = false;
                _audioSource.loop = false;
            }

            return _audioSource;
        }
    }

    private void Update() {
        if (isPlaying && playlist && playlist.tracks.Count > 0 && currentTrackIndex < playlist.tracks.Count && audioSource.clip) {
            if (!audioSource.isPlaying)
                NextTrack();
        }
    }

    private void OnEnable() {
        InitializeAndPlay();
    }

    public void InitializeAndPlay() {
        playlist = AssetDatabase.LoadAssetAtPath<YG_MusicPlaylist>(PlaylistAsset);
        audioSource = null;
        
        if (!audioSource) {
            audioSource = AudioSource;
        }
        
        if (playlist && playlist.autoPlay && playlist.tracks.Count > 0) {
            PlayCurrentTrack();
            isPlaying = true;
        }
    }

    private void OnGUI() {
        GUILayout.Label("YG Music Playlist", EditorStyles.boldLabel);
        
        if (playlist) {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("⏮", GUILayout.Width(40))) PreviousTrack();
            if (GUILayout.Button(isPlaying ? "⏸" : "▶", GUILayout.Width(40))) PlayPauseToggle();
            if (GUILayout.Button("⏭", GUILayout.Width(40))) NextTrack();
            EditorGUILayout.EndHorizontal();
            
            if (playlist.tracks.Count > 0 && currentTrackIndex < playlist.tracks.Count)
                GUILayout.Label("Now Playing: " + playlist.tracks[currentTrackIndex].name);
            
            float currentTime = audioSource.time;
            float totalTime = audioSource.clip.length;
            currentTime = EditorGUILayout.Slider(currentTime, 0f, totalTime);
            audioSource.time = currentTime;
            
            string currentTimeString = FormatTime(currentTime);
            string totalTimeString = FormatTime(totalTime);
            
            GUILayout.Label(currentTimeString + " / " + totalTimeString);
            
            audioSource.volume = EditorGUILayout.Slider("Volume", audioSource.volume, 0f, 1f);
            
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();


            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        } else {
            EditorGUILayout.HelpBox("Create a Music Playlist asset.", MessageType.Info);
            
            if (GUILayout.Button("Create Playlist")) {
                playlist = CreateInstance<YG_MusicPlaylist>();
                AssetDatabase.CreateAsset(playlist, PlaylistAsset);
                AssetDatabase.SaveAssets();
            }
        }
    }


    private void PlayPauseToggle() {
        isPlaying = !isPlaying;
        
        if (isPlaying && playlist.tracks.Count > 0 && currentTrackIndex < playlist.tracks.Count) {
            audioSource.clip = playlist.tracks[currentTrackIndex];
            audioSource.loop = false;
            audioSource.Play();
        } else {
            audioSource.Pause();
        }
    }

    private void NextTrack() {
        if (playlist.tracks.Count > 0) {
            currentTrackIndex = (currentTrackIndex + 1) % playlist.tracks.Count;
            PlayCurrentTrack();
        }
    }

    private void PreviousTrack() {
        if (playlist.tracks.Count > 0) {
            currentTrackIndex = (currentTrackIndex - 1 + playlist.tracks.Count) % playlist.tracks.Count;
            PlayCurrentTrack();
        }
    }
    
    private void AdjustVolume(float delta) {
        audioSource.volume = Mathf.Clamp01(audioSource.volume + delta);
    }
    
    private void PlayCurrentTrack() {
        if (isPlaying && playlist.tracks.Count > 0 && currentTrackIndex < playlist.tracks.Count) {
            audioSource.clip = playlist.tracks[currentTrackIndex];
            audioSource.loop = false;
            audioSource.Play();
        }
    }
    
    private string FormatTime(float time) {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}