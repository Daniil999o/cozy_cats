using System.Linq;
using UnityEngine;

public class RoomObjectSkinChanger : MonoBehaviour
{
    [SerializeField] private bool autoSet;
    
    [SerializeField] private RoomObject mainObject;

    public string ElementId => mainObject.ElementId;
    
    [Space]
    
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private CustomSkinsSO myCustomSkins;

    public CustomSkinSO[] All => myCustomSkins.All;

#if UNITY_EDITOR

    private void OnValidate() {
        if (autoSet) {
            mainObject = GetComponent<RoomObject>();
            meshFilter = GetComponent<MeshFilter>();
        }

        autoSet = false;
    }
    
#endif
    
    private void Start() {
        UpdateSkin();
    }

    public void UpdateSkin() {
        string myId = RoomShop.GetObjectSkin(ElementId);

        if (myId.IsEmptyOrWhiteSpaces()) {
            myId = All[0].id;

            RoomShop.BuyCustomSkin(myId);
            RoomShop.SetObjectSkin(ElementId, myId);
        }
        
        UpdateSkin(myId);
    }
    
    private void UpdateSkin(string currentId) {
        CustomSkinSO mySkin = myCustomSkins.GetSkinSO(currentId);
        
        if (!mySkin) return;
        
        meshFilter.mesh = mySkin.mesh;
    }
}