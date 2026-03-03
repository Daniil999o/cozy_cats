using UnityEngine;

[CreateAssetMenu(fileName = "Custom Material", menuName = "Room Customization/Custom Material")]
public class CustomMaterialSO : ScriptableObject
{
    public string id;
    public Material material;
    public Mesh mesh;
    [SerializeField] private Sprite customPreview;
    public int price;

    public Sprite Preview {
        get {
            if (customPreview) return customPreview;
            
            Texture2D texture = (Texture2D)material.mainTexture;
            Sprite preview = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            return preview;
        }
    }
}