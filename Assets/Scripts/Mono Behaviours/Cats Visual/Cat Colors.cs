using UnityEngine;
using CozyPolyomino;

public class CatColors : MonoBehaviour
{
    [field: SerializeField] public bool Hover { get; set; }

    [field: Space]

    [SerializeField] private GameObject[] myObjects;
    
    [Space]
    
    [SerializeField] private string catSelectLayer;
    [SerializeField] private string catErrorLayer;
    [SerializeField] private string catCorrectLayer;
    [SerializeField] private string catDefaultLayer;

    private void Start() {
        SetDefault();
    }
    
    private void SetSelect() => ChangeLayer(catSelectLayer);
    
    private void SetError() => ChangeLayer(catErrorLayer);
    
    private void SetCorrect() => ChangeLayer(catCorrectLayer);

    private void SetDefault() => ChangeLayer(catDefaultLayer);

    private void ChangeLayer(string layerName) {
        for (int i = 0; i < myObjects.Length; i++) myObjects[i].layer = LayerMask.NameToLayer(layerName);
    }

    public void UpdateState(DraggableShape shape) {
        if (!Hover && !shape.IsDragging) {
            SetDefault();
            return;
        }

        bool onGrid = shape.HoverGrid;
        bool isValid = shape.IsValid;

        if (onGrid) {
            if (isValid)
                SetCorrect();
            else
                SetError();
            
            return;
        }
        
        SetSelect();
    }
}