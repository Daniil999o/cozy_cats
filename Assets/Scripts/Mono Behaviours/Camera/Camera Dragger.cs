using UnityEngine;
using UnityEngine.EventSystems;

public class CameraDragger : MonoBehaviour, IDragHandler
{
    [SerializeField] private CameraController cameraController;
    
    public void OnDrag(PointerEventData eventData) {
        cameraController.Move(eventData.delta);
    }
}