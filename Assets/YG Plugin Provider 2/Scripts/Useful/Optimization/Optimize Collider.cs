using UnityEngine;

[ExecuteInEditMode]
public class OptimizeCollider : MonoBehaviour
{
    private void Awake() {
        foreach (var x in GetComponents<BoxCollider>()) {
            x.center = x.center.Optimize(2);
            x.size = x.size.Optimize(2);
        }

        foreach (var x in GetComponents<CapsuleCollider>()) {
            x.center = x.center.Optimize(2);
            x.radius = x.radius.Optimize(2);
            x.height = x.height.Optimize(2);
        }

        foreach (var x in GetComponents<SphereCollider>()) {
            x.center = x.center.Optimize(2);
            x.radius = x.radius.Optimize(2);
        }
    }

    private void Start() => DestroyImmediate(this);
}