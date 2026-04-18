using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace CozyPolyomino
{
    public class DraggableShape : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        [Header("Data")]
        [SerializeField] internal bool isBig;
        [SerializeField] internal ShapeData shapeData;

        [Space]

        [SerializeField] private Transform dragModel;
        [SerializeField] private Transform center;

        internal Vector3 Center => center ? center.position : _transform.position;

        [Space]

        [SerializeField] private UnityEvent<DraggableShape> onEnterHover;
        [SerializeField] private UnityEvent<DraggableShape> onExitHover;
        [SerializeField] private UnityEvent<DraggableShape> onStartDrag;
        [SerializeField] private UnityEvent<DraggableShape> onDragging;
        [SerializeField] private UnityEvent<DraggableShape> onEndDrag;

        private Vector2Int[] _currentShapeCells;
        private bool _isDragging;
        private bool _isOnGrid;
        private bool _isPreviewValid;
        private Vector3 _startPosition;
        private Vector2Int _lastGridPos;
        private Vector3 _previewWorldPos;
        private Plane _dragPlane;
        private Vector3 _dragOffset;

        public bool Placed => _isOnGrid;

        private GridController _gridController;
        private DraggableOptionsData _dragOptions;
        private float _cellSize;

        private Transform _transform;
        
        public bool IsValid => _isPreviewValid;
        public bool IsDragging => _isDragging;
        
        public bool HoverGrid { get; set; }

        private float _defaultY;
        private float _yOffset;

        public Action<DraggableShape> OnEnterHover;
        public Action<DraggableShape> OnExitHover;
        public Action<DraggableShape> OnStartDrag;
        public Action<DraggableShape> OnDragging;
        public Action<DraggableShape> OnEndsDrag;

        internal Action OnPlaced;
        private Action<DraggableShape, bool> _onDrop;
        
        private static DraggableShape CurrentDraggable { get; set; }

        public Vector2Int[] GetCellsOnGrid() {
            Vector3 targetPos = _transform.position;
            targetPos.y = _startPosition.y;

            Vector2Int gridPos = _gridController.WorldToGrid(targetPos);

            gridPos -= new Vector2Int(1, 1);
            
            if (isBig)
                print($"Origin: {gridPos}, Cells: {string.Concat(_currentShapeCells.Select(x => x + gridPos).ToArray())}");

            return _currentShapeCells.Select(x => x + gridPos).ToArray();
        }

        private void Awake() {
            _transform = GetComponent<Transform>();

            if (dragModel) _defaultY = dragModel.localPosition.y;
            
            _gridController = GridController.Instance;
            _dragOptions = GridController.DraggableOptions;
        }

        private void OnEnable() {
            _onDrop += GameEffects.Drop;
        }

        private void OnDisable() {
            _onDrop -= GameEffects.Drop;
        }

        private void Start() {
            _startPosition = _transform.position;
            _cellSize = _gridController.cellSize;
            _currentShapeCells = shapeData.GetCells();
        }

        private void Update() {
            if (dragModel) {
                Vector3 newPos = dragModel.localPosition;
                newPos.y = _defaultY + _yOffset;

                dragModel.localPosition = Vector3.Lerp(dragModel.localPosition, newPos, 6f * Time.deltaTime);
            }

            if (!_isDragging) return;

            ProcessDrag(Pointer.current?.position.ReadValue() ?? Vector2.zero);
        }
        
        public void OnPointerEnter(PointerEventData eventData) {
            onEnterHover?.Invoke(this);
            OnEnterHover?.Invoke(this);
            
            if (!CurrentDraggable || CurrentDraggable == this) _yOffset = 1f;
        }
        
        public void OnPointerExit(PointerEventData eventData) {
            onExitHover?.Invoke(this);
            OnExitHover?.Invoke(this);
            
            if (!CurrentDraggable || CurrentDraggable != this) _yOffset = 0f;
        }

        public void DebugFromMe(string message) => Debug.Log($"{name}: {message}");

        public void OnBeginDrag(PointerEventData eventData) {
            _yOffset = 1f;

            CurrentDraggable = this;
            
            _isDragging = true;

            _dragPlane = new Plane(Vector3.up, _transform.position);
            _dragOffset = _transform.position - GetMouseWorldPos(eventData.position);
            
            onStartDrag?.Invoke(this);
            OnStartDrag?.Invoke(this);

            if (!_isOnGrid) return;

            _gridController.RemoveShape(_lastGridPos, _currentShapeCells);
            _isOnGrid = false;
        }

        private void ProcessDrag(Vector2 screenPos) {
            Vector3 rawMousePos = GetMouseWorldPos(screenPos);
            Vector3 targetPos = rawMousePos + _dragOffset;

            targetPos.y = _startPosition.y + _dragOptions.liftHeight;

            Vector2Int pivotGridPos = _gridController.WorldToGrid(targetPos);

            _isPreviewValid = _gridController.CanPlaceShape(pivotGridPos, _currentShapeCells);

            Vector3 snappedPos = _gridController.GridToWorldCenter(pivotGridPos);

            snappedPos.y = _gridController.transform.position.y;

            _previewWorldPos = snappedPos;

            HoverGrid = GridDrawer.Instance.HighlightPreview(pivotGridPos, shapeData.GetCells(), _isPreviewValid);

            if (_isPreviewValid) {
                Vector3 snapTarget = snappedPos;
                snapTarget.y = targetPos.y;
                targetPos = Vector3.Lerp(targetPos, snapTarget, _dragOptions.snapStrength);
            }

            _transform.position = Vector3.Lerp(
                _transform.position,
                targetPos,
                _dragOptions.dragSpeed * Time.deltaTime
            );
            
            onDragging?.Invoke(this);
            OnDragging?.Invoke(this);
        }

        public void OnEndDrag(PointerEventData eventData) {
            _yOffset = 0f;

            if (CurrentDraggable == this) CurrentDraggable = null;
            
            _isDragging = false;

            Vector3 rawMousePos = GetMouseWorldPos(eventData.position);
            Vector3 targetPos = rawMousePos + _dragOffset;
            targetPos.y = _startPosition.y + _dragOptions.liftHeight;

            Vector2Int pivotGridPos = _gridController.WorldToGrid(targetPos);

            bool isCorrect = _gridController.CanPlaceShape(pivotGridPos, _currentShapeCells);

            if (isCorrect)
                PlaceShape(pivotGridPos);
            else
                _transform.position = GridController.Instance.GetOutOfGridPos(this, _currentShapeCells);

            GridDrawer.Instance.ResetGridColors();
            
            onEndDrag?.Invoke(this);
            OnEndsDrag?.Invoke(this);
            HoverGrid = false;
            
            _onDrop?.Invoke(this, isCorrect);
        }

        private void PlaceShape(Vector2Int gridPos) {
            Vector3 worldPlacePos = _gridController.GridToWorldCenter(gridPos);
            worldPlacePos.y = _gridController.transform.position.y;

            _transform.position = worldPlacePos;
            _gridController.PlaceShape(gridPos, _currentShapeCells);

            _lastGridPos = gridPos;
            _isOnGrid = true;
            
            OnPlaced?.Invoke();

            // Sound: PLOP
        }

        private Vector3 GetMouseWorldPos(Vector2 screenPos) {
            Ray ray = GridController.MainCamera.ScreenPointToRay(screenPos);

            return _dragPlane.Raycast(ray, out float distance) ? ray.GetPoint(distance) : _transform.position;
        }

        public void OnDrag(PointerEventData eventData) { }

#if UNITY_EDITOR

        private void OnDrawGizmos() {
            if (!_gridController || !_gridController.Debug) return;

            Vector2Int[] cells = _currentShapeCells is { Length: > 0 } ? _currentShapeCells : shapeData?.GetCells();

            if (cells == null) return;

            float drawCellSize = _cellSize;
            Vector3 drawOrigin;
            Color gizmoColor;

            if (_isDragging && Application.isPlaying) {
                drawOrigin = _previewWorldPos;
                gizmoColor = _isPreviewValid ? Color.green : Color.red;
            } else {
                drawOrigin = _transform.position;
                gizmoColor = Color.cyan;
            }

            Gizmos.color = gizmoColor;
            float cubeSize = drawCellSize * 0.95f;

            foreach (Vector2Int cell in cells) {
                Vector3 cellWorldPos = drawOrigin + new Vector3(
                    cell.x * drawCellSize,
                    0.1f,
                    cell.y * drawCellSize
                );
                Gizmos.DrawWireCube(cellWorldPos, new Vector3(cubeSize, 0.1f, cubeSize));
            }
        }

#endif
    }
}