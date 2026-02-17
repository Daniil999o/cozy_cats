using UnityEngine;

[System.Serializable]
public class YG_IndexedMaterial
{
    [SerializeField] private Renderer renderer;
    [SerializeField] private int materialIndex;

    private Material _myMaterial;
    
    public Material Material {
        get {
            if (!_myMaterial) {
                Material[] mats = renderer.materials;
                mats[materialIndex] = _myMaterial = new Material(mats[materialIndex]);
                renderer.materials = mats;
            }

            return _myMaterial;
        }
    }
}