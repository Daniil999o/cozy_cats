using UnityEngine;

[ExecuteInEditMode]
public class RandomGrid : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private bool destroyOnStart;

    [Space]

    [Header("Grid Options")]
    [SerializeField] private Transform[] points;

    private void Start() {
        if (destroyOnStart)
            Destroy(this);
        ArrangeChildren();
    }

    private void OnValidate() {
        ArrangeChildren();
    }

    public void ArrangeChildren() {
        int point = 0;

        foreach (var x in GetComponentsInChildren<GridElement>()) {
            x.transform.SetParent(points[point]);
            x.transform.localPosition = Vector3.zero;

            point++;

            if (point >= points.Length) point = 0;
        }
    }

    public void AddChild(Transform child) {
        child.SetParent(transform);
        ArrangeChildren();
    }
}