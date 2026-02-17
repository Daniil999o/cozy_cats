using UnityEngine;

[ExecuteInEditMode]
public class OptimizeTransform : MonoBehaviour
{
    private void Awake() {
        transform.localPosition = transform.localPosition.Optimize(2);
        transform.localRotation = Quaternion.Euler(transform.localEulerAngles.Optimize(0));
        transform.localScale = transform.localScale.Optimize(2);
    }

    private void Start() => DestroyImmediate(this);
}