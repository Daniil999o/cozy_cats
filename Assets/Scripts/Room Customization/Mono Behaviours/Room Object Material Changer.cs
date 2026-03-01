using Sirenix.Utilities;
using System.Linq;
using UnityEngine;

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
        
        Debug.Log($"First: {myId}");

        if (myId.IsNullOrWhitespace()) {
            myId = FirstMaterial.All[0].id;

            RoomShop.BuyCustomMaterial(myId);
            RoomShop.SetObjectMaterial(ElementId, myId);
        }
        
        Debug.Log($"Next: {myId}");
        
        UpdateMaterial(myId);
    }
    
    private void UpdateMaterial(string currentId) {
        Material[] renderMaterials = customRenderer.materials;

        for (int i = 0; i < renderMaterials.Length; i++) {
            CustomIndexedMaterial collectedMat = customIndexedMaterials.FirstOrDefault(x => x.index == i);

            if (collectedMat != null)
                renderMaterials[i] = collectedMat.customMaterials.GetMaterial(currentId);
        }
        
        customRenderer.materials = renderMaterials;
    }
}