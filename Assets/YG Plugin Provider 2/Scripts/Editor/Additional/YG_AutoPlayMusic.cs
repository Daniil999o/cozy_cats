using UnityEditor;

[InitializeOnLoad]
public class YG_AutoPlayMusic
{
    static YG_AutoPlayMusic() {
        EditorApplication.update += StartMusic;
    }
    
    private static void StartMusic() {
        YG_MusicPlaylistEditor window = EditorWindow.GetWindow<YG_MusicPlaylistEditor>();

        if (window) {
            window.InitializeAndPlay();
            window.Close();
        }

        EditorApplication.update -= StartMusic;
    }
}