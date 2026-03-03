using UnityEngine;
using UnityEngine.EventSystems;

public class CameraDragger : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private bool useRot;
    
    [Space]
    
    [SerializeField] private CameraController cameraController;
    
    public Vector2 Delta { get; private set; }
    public Vector2 Pointer { get; private set; }
    public bool Dragging { get; private set; }

    public void OnBeginDrag(PointerEventData eventData) {
        Dragging = true;
        Delta = Vector2.zero;
    }

    public void OnEndDrag(PointerEventData eventData) {
        Dragging = false;
        Delta = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData) {
        Delta = eventData.delta;
        
        if (!cameraController) return;
        
        if (useRot)
            cameraController.Rotate(Delta);
        else
            cameraController.Move(Delta);
    }

    public void OnPointerDown(PointerEventData eventData) {
        Pointer = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData) {
        Pointer = eventData.position;
    }
}