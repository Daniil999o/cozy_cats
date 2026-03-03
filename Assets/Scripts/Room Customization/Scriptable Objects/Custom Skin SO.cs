using UnityEngine;

[CreateAssetMenu(fileName = "Custom Skin", menuName = "Room Customization/Custom Skin")]
public class CustomSkinSO : ScriptableObject
{
    public string id;
    public Mesh mesh;
    public int price;

    [Space]
    
    [Header("Preview")]
    [SerializeField] private Vector3 autoPreviewOffset = new Vector3(-1f, 0f, 0f);
    [SerializeField] private Vector3 autoPreviewLookOffset = new Vector3(0f, 0.2f, 0f);
    [SerializeField] private Sprite customPreview;
    
    public Sprite Preview {
        get {
            if (customPreview) return customPreview;

            return RenderMeshPreview();
        }
    }

    private Sprite RenderMeshPreview() {
        const int previewSize = 512;

        GameObject previewObject = new GameObject("PreviewMesh");
        previewObject.AddComponent<MeshFilter>().mesh = mesh;

        Material material = new Material(Shader.Find("Custom/URP/Wireframe"));

        material.SetColor("_WireColor", Color.black);
        material.SetColor("_FillColor", new Color(0.27f, 0.89f, 0.43f, 1f));
        material.SetFloat("_WireWidth", 0.05f);

        previewObject.AddComponent<MeshRenderer>().material = material;

        previewObject.layer = 18;

        Bounds bounds = mesh.bounds;
        previewObject.transform.position = -bounds.center;
        previewObject.transform.rotation = Quaternion.Euler(new Vector3(0f, 45f, 0f));

        RenderTexture rt = new RenderTexture(
            previewSize,
            previewSize,
            24,
            RenderTextureFormat.ARGB32
        );

        GameObject cameraGO = new GameObject("PreviewCamera");

        Camera cam = cameraGO.AddComponent<Camera>();

        cam.cullingMask = 1 << 18;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.55f, 1f, 0.88f, 1f);
        cam.orthographic = false;
        cam.fieldOfView = 30f;
        cam.targetTexture = rt;
        cam.enabled = false;

        float objectSize = bounds.extents.magnitude * 2f;
        float distance = objectSize / (2f * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad));
        distance *= 1f;

        cameraGO.transform.position = bounds.center + autoPreviewOffset * distance;

        Vector3 lookPos = bounds.center;
        lookPos.x *= autoPreviewLookOffset.x;
        lookPos.y *= autoPreviewLookOffset.y;
        lookPos.z *= autoPreviewLookOffset.z;
        
        cameraGO.transform.LookAt(lookPos);

        cam.Render();

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(previewSize, previewSize, TextureFormat.RGBA32, false);
        
        tex.ReadPixels(new Rect(0, 0, previewSize, previewSize), 0, 0);
        tex.Apply();
        RenderTexture.active = null;

        DestroyImmediate(previewObject);
        DestroyImmediate(cameraGO);
        DestroyImmediate(rt);

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

        return sprite;
    }
}