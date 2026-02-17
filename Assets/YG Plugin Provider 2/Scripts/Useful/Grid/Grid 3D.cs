using UnityEngine;

[ExecuteInEditMode]
public class Grid3D : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private bool destroyOnStart;

    [Space]

    [Header("Grid Options")]
    [SerializeField] private Vector3 offsets = new Vector3(1f, 1f, 1f);

    [SerializeField][Range(1, 30)] private int fixedColumns = 1;
    [SerializeField][Range(1, 30)] private int fixedRows = 1;
    [SerializeField][Range(1, 30)] private int fixedDepth = 1;

    [Space]

    [Header("Alignment")]
    [SerializeField] private AlignmentX xAlignment = AlignmentX.Center;
    [SerializeField] private AlignmentY yAlignment = AlignmentY.Center;
    [SerializeField] private AlignmentZ zAlignment = AlignmentZ.Center;
    
    [Space]

    [Header("Rotation")]
    [SerializeField] private bool useFixedRotation;
    [SerializeField] private Vector3 fixedRotation = Vector3.zero;
    
    
    private enum AlignmentX { Left, Center, Right }
    private enum AlignmentY { Bottom, Center, Top }
    private enum AlignmentZ { Back, Center, Forward }

    private void Start() {
        if (destroyOnStart)
            Destroy(this);
    }

    private void OnValidate() {
        if (transform.childCount == 0) return;
        
        int totalCells = fixedColumns * fixedRows * fixedDepth;
        int childIndex = 0;
        
        float gridWidth = (fixedColumns - 1) * offsets.x;
        float gridHeight = (fixedRows - 1) * offsets.y;
        float gridDepth = (fixedDepth - 1) * offsets.z;
        
        float xOffset = 0f;
        float yOffset = 0f;
        float zOffset = 0f;
        
        switch (xAlignment) {
            case AlignmentX.Right: xOffset = -gridWidth; break;
            case AlignmentX.Center: xOffset = -gridWidth / 2f; break;
        }
        
        switch (yAlignment) {
            case AlignmentY.Top: yOffset = -gridHeight; break;
            case AlignmentY.Center: yOffset = -gridHeight / 2f; break;
        }
        
        switch (zAlignment) {
            case AlignmentZ.Forward: zOffset = -gridDepth; break;
            case AlignmentZ.Center: zOffset = -gridDepth / 2f; break;
        }
        
        for (int z = 0; z < fixedDepth; z++) {
            for (int y = 0; y < fixedRows; y++) {
                for (int x = 0; x < fixedColumns; x++) {
                    if (childIndex >= transform.childCount) break;
                    
                    Transform child = transform.GetChild(childIndex);
                    
                    Vector3 position = transform.position + new Vector3(x * offsets.x + xOffset,
                        y * offsets.y + yOffset, z * offsets.z + zOffset
                    );
                    child.position = position;
                    
                    if (useFixedRotation)
                        child.rotation = Quaternion.Euler(fixedRotation);
                    
                    childIndex++;
                }
            }
        }
    }
}