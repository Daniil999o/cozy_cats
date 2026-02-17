using UnityEngine;

public class YG_ObjectSizerUI : MonoBehaviour
{
    [SerializeField][Range(0f, 1f)] private float scale = 1f;
    [SerializeField] private Vector2 defaultObjectSize = new Vector2(100f, 100f);

    private void OnValidate() {
        GetComponent<RectTransform>().sizeDelta = defaultObjectSize * scale;
    }
}