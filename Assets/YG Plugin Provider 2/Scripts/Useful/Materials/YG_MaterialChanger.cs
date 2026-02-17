using System.Collections.Generic;
using UnityEngine;

public class YG_MaterialChanger : MonoBehaviour
{
    [SerializeField] private bool autoSetup;
    
    [Space]
    
    [SerializeField] private bool choiceRandomOnStart = true;
    [SerializeField] private Renderer myRenderer;

    [Space]

    [SerializeField] private MaterialToChange[] materials;

    [System.Serializable]
    private struct MaterialToChange
    {
        public int materialIndex;
        public Material[] materials;
    }

    private int _currentMaterialsIndex;

    private void OnValidate() {
        if (autoSetup) {
            autoSetup = false;

            if (materials.Length > 0) {
                Debug.LogError("Materials array is not empty");
                return;
            }

            myRenderer = GetComponent<Renderer>();

            List<MaterialToChange> materialsToChange = new List<MaterialToChange>();
            Material[] myMaterials = myRenderer.sharedMaterials;

            for (int i = 0; i < myMaterials.Length; i++) {
                materialsToChange.Add(new MaterialToChange {
                        materialIndex = i,
                        materials = new[] {
                            myMaterials[i]
                        }
                    }
                );
            }

            materials = materialsToChange.ToArray();
        }
    }

    private void Start() {
        if (!choiceRandomOnStart) {
            SetMaterialsIndex(0);
            
            return;
        }
        
        Material[] myMaterials = myRenderer.materials;
        
        foreach (var x in materials)
            myMaterials[x.materialIndex] = x.materials.Choice();

        myRenderer.materials = myMaterials;
    }

    public void NextMaterial() {
        _currentMaterialsIndex++;
        SetMaterialsIndex(_currentMaterialsIndex);
    }

    public void PreviousMaterial() {
        _currentMaterialsIndex--;
        SetMaterialsIndex(_currentMaterialsIndex);
    }

    public void SetMaterialsIndex(int index) {
        Material[] myMaterials = myRenderer.materials;

        foreach (var x in materials) {
            if (myMaterials.InLength(x.materialIndex) && x.materials.InLength(index))
                myMaterials[x.materialIndex] = x.materials[index];
        }

        myRenderer.materials = myMaterials;

        _currentMaterialsIndex = index;
    }
}