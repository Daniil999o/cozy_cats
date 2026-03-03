using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Custom Skins", menuName = "Room Customization/Custom Skins")]
public class CustomSkinsSO : ScriptableObject
{
    [SerializeField] private CustomSkinSO[] myCustomSkins;

    public CustomSkinSO[] All => myCustomSkins;

    public CustomSkinSO GetSkinSO(string id) =>
        myCustomSkins.FirstOrDefault(x => x.id == id);
}