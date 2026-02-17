using UnityEngine;

public class YG_ObjectSizer : MonoBehaviour
{
    [SerializeField][Range(0f, 1f)] private float scale = 1f;
    [SerializeField] private Vector3 defaultObjectSize = new Vector3(1f, 1f, 1f);

    private void OnValidate() {
        transform.localScale = defaultObjectSize * scale;
    }
}