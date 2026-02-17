using UnityEngine;

[RequireComponent(typeof(Camera))]
public class YG_DownsampleRenderer : MonoBehaviour
{
    [SerializeField][Range(0.001f, 2.0f)] private float renderScale = 1f;

    private void OnRenderImage(RenderTexture src, RenderTexture dest) {
        int renderWidth = Mathf.RoundToInt(src.width * renderScale);
        int renderHeight = Mathf.RoundToInt(src.height * renderScale);
        
        if (renderWidth <= 0 || renderHeight <= 0) {
            Graphics.Blit(src, dest);
            return;
        }
        
        RenderTexture rt = RenderTexture.GetTemporary(renderWidth, renderHeight, 0);
        
        Graphics.Blit(src, rt);
        Graphics.Blit(rt, dest);
        
        RenderTexture.ReleaseTemporary(rt);
    }
}