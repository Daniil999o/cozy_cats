using System.Collections.Generic;
using UnityEngine;

public class SpinningSlices : MonoBehaviour
{
    [Header("For Test")]
    [SerializeField] private bool test;
    [SerializeField] private int testCount = 8;
    [SerializeField] private float width = 200f;
    [SerializeField] private float height = 265f;
    [SerializeField] private RectTransform original;
    [SerializeField] private List<RectTransform> testSlices;

    private void SetAngle(RectTransform t, float angle) {
        Vector3 angles = t.localEulerAngles;
        angles.z = angle;
        
        t.localRotation = Quaternion.Euler(angles);
    }
    
    private void OnValidate() {
        if (!test) return;

        test = false;
        
        testSlices.ForEach(x => DestroyImmediate(x.gameObject));
        testSlices.Clear();

        float angle = 360f / testCount;
        
        SetAngle(original, 0f);
        original.sizeDelta = new Vector2(width, height);
        
        float currentAngle = 0;
        
        for (int i = testCount; i > 0; i--) {
            RectTransform duplicate = Instantiate(original, original.transform.parent);
            duplicate.sizeDelta = new Vector2(width, height);
            
            currentAngle += angle;
            SetAngle(duplicate, currentAngle);
            
            testSlices.Add(duplicate);
        }
    }
}