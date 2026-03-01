using UnityEngine;
using UnityEngine.EventSystems;

public class CameraDragger : MonoBehaviour, IDragHandler
{
    [SerializeField] private bool useRot;
    [SerializeField] private CameraController cameraController;
    
    public void OnDrag(PointerEventData eventData) {
        if (useRot)
            cameraController.Rotate(eventData.delta);
        else
            cameraController.Move(eventData.delta);
    }
}