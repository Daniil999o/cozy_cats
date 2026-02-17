using UnityEngine;
using UnityEngine.EventSystems;

public class CameraMover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private CameraController cameraController;

    [Space]

    [SerializeField] private float sense = 3f;
    [SerializeField] private Vector2 myDelta;

    private bool _entered;

    private void Update() {
        if (!_entered) return;
        
        cameraController.Move(-myDelta * sense);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        _entered = true;
    }
    public void OnPointerExit(PointerEventData eventData) {
        _entered = false;
    }
}