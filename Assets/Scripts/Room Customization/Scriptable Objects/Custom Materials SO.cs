using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Custom Material", menuName = "Room Customization/Custom Materials")]
public class CustomMaterialsSO : ScriptableObject
{
    [SerializeField] private CustomMaterialSO[] myCustomMaterials;

    public CustomMaterialSO[] All => myCustomMaterials;
    
    public Material GetMaterial(int index) => myCustomMaterials[index].material;

    public Material GetMaterial(string id) => myCustomMaterials.FirstOrDefault(x => x.id == id)?.material;
}