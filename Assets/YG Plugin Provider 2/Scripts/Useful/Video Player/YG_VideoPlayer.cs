using System.IO;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class YG_VideoPlayer : MonoBehaviour
{
    [Header("File")]
    [SerializeField] private string videoName;
    [SerializeField] private VideoType videoType = VideoType.MP4;

    [Space]

    [Header("Video Playback")]
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private bool loop = true;

    private enum VideoType
    {
        MP4,
        MP3,
        AVI,
        MOV,
        MKV,
        FLV
    }
    
    private VideoPlayer _player;

    private void Awake() {
        _player = GetComponent<VideoPlayer>();

        _player.url = Path.Combine(Application.streamingAssetsPath, $"{videoName}.{videoType.ToString().ToLowerInvariant()}");

        _player.isLooping = loop;
        _player.playOnAwake = playOnAwake;
        
        if (playOnAwake)
            _player.Play();
        else
            _player.Stop();
    }
}