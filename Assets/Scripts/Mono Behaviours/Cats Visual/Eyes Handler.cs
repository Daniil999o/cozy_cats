using UnityEngine;
using CozyPolyomino;
using UnityEngine.InputSystem;

public class EyesHandler : MonoBehaviour
{
    public static Vector3 LookPos;
    
    [SerializeField] private Transform target;
    [SerializeField] private float distanceFromCamera = 10f;

    private void Update() {
        if (Pointer.current == null) return;
        
        Vector2 screenPosition = Pointer.current.position.ReadValue();

        Vector3 screenPos = new Vector3(screenPosition.x, screenPosition.y, distanceFromCamera);
        
        LookPos = GridController.MainCamera.ScreenToWorldPoint(screenPos);

        if (target) target.position = LookPos;
    }
}