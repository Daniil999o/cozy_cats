using UnityEngine;

[CreateAssetMenu(fileName = "Shop Item", menuName = "Room Customization/Shop Item")]
public class ShopItemSO : ScriptableObject
{
    public string id;
    public RoomObject objectPrefab;
    public int price;

    [Space]
    
    [Header("Preview")]
    [SerializeField] private Vector3 autoPreviewOffset = new Vector3(-1f, 0f, 0f);
    [SerializeField] private Vector3 autoPreviewLookOffset = new Vector3(0f, 0.2f, 0f);
    [SerializeField] private Sprite customPreview;

    [Space]

    [SerializeField] private bool previewTest;
    
    public Sprite Preview {
        get {
            if (customPreview) return customPreview;

            return RenderMeshPreview();
        }
    }

    private Sprite RenderMeshPreview() {
        const int previewSize = 512;

        GameObject previewObject = Instantiate(objectPrefab).gameObject;

        previewObject.layer = 18;
        for (int i = 0; i < previewObject.transform.childCount; i++)
            previewObject.transform.GetChild(i).gameObject.layer = 18;

        MeshFilter[] meshFilters = previewObject.GetComponentsInChildren<MeshFilter>(true);
        
        if (meshFilters.Length <= 0) return null;
        
        Bounds bounds = meshFilters[0].sharedMesh.bounds;
        
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
        cam.clearFlags = CameraClearFlags.Depth;
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

        if (!previewTest) {
            DestroyImmediate(previewObject);
            DestroyImmediate(cameraGO);
        }

        DestroyImmediate(rt);

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

        return sprite;
    }
}