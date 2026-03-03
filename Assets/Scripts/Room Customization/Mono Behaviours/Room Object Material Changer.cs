using UnityEngine;
using System.Linq;

public class RoomObjectMaterialChanger : MonoBehaviour
{
    [SerializeField] private bool autoSet;
    
    [SerializeField] private RoomObject mainObject;

    public string ElementId => mainObject.ElementId;
    
    [Space]
    
    [SerializeField] private Renderer customRenderer;

    [Space]

    [SerializeField] private CustomIndexedMaterial[] customIndexedMaterials;

    public CustomMaterialsSO FirstMaterial => customIndexedMaterials.First().customMaterials; 

    [System.Serializable]
    private class CustomIndexedMaterial
    {
        public int index;
        public CustomMaterialsSO customMaterials;
    }

#if UNITY_EDITOR

    private void OnValidate() {
        if (autoSet) {
            mainObject = GetComponent<RoomObject>();
            customRenderer = GetComponent<Renderer>();
        }

        autoSet = false;
    }
    
#endif
    
    private void Start() {
        UpdateMaterial();
    }

    public void UpdateMaterial() {
        string myId = RoomShop.GetObjectMaterial(ElementId);

        if (myId.IsEmptyOrWhiteSpaces()) {
            myId = FirstMaterial.All[0].id;

            RoomShop.BuyCustomMaterial(myId);
            RoomShop.SetObjectMaterial(ElementId, myId);
        }
        
        UpdateMaterial(myId);
    }
    
    private void UpdateMaterial(string currentId) {
        Material[] renderMaterials = customRenderer.materials;

        for (int i = 0; i < renderMaterials.Length; i++) {
            CustomIndexedMaterial collectedMat = customIndexedMaterials.FirstOrDefault(x => x.index == i);

            if (collectedMat == null) continue;
            
            CustomMaterialSO myMat = collectedMat.customMaterials.GetMaterialSO(currentId);

            if (myMat.mesh) {
                GetComponent<MeshFilter>().mesh = myMat.mesh;
                    
                return;
            }
            renderMaterials[i] = myMat.material;
        }
        
        customRenderer.materials = renderMaterials;
    }
}