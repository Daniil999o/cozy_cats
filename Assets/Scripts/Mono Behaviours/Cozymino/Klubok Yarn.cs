using UnityEngine;
using CozyPolyomino;
using System.Collections.Generic;

public class KlubokYarn : BonusCell
{
    [Space]
    
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] private Vector3 movePos;

    private Transform _transform;
    private bool _move;
    
    private void Awake() {
        _transform = GetComponent<Transform>();
    }

    public override void NotifyGridChanged(Vector2Int[] draggablePositions, Vector2Int[] obstaclePositions) {
        base.NotifyGridChanged(draggablePositions, obstaclePositions);

        Vector2Int oldPos = OnGridPosition;

        HashSet<Vector2Int> obstacles = new HashSet<Vector2Int>(obstaclePositions);
        HashSet<Vector2Int> draggables = new HashSet<Vector2Int>(draggablePositions);

        Vector2Int[] directions = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        List<Vector2Int> adjacentDraggables = new List<Vector2Int>();

        foreach (Vector2Int dir in directions) {
            Vector2Int neighbour = oldPos + dir;
            if (draggables.Contains(neighbour)) adjacentDraggables.Add(neighbour);
        }

        if (adjacentDraggables.Count == 0) {
            movePos = Vector3.zero;
            return;
        }

        Vector2 desiredDir = Vector2.zero;
        
        foreach (Vector2Int dragPos in adjacentDraggables) {
            Vector2 away = oldPos - dragPos;
            desiredDir += away.normalized;
        }

        desiredDir.Normalize();

        Vector2Int bestMove = Vector2Int.zero;
        float bestScore = float.NegativeInfinity;
        bool anyValid = false;

        foreach (Vector2Int dir in directions) {
            Vector2Int newPos = oldPos + dir;

            if (obstacles.Contains(newPos) || draggables.Contains(newPos)) continue;

            float score = Vector2.Dot(dir, desiredDir);

            if (!(score > bestScore)) continue;
            
            bestScore = score;
            bestMove = dir;
            anyValid = true;
        }

        if (anyValid) OnGridPosition = oldPos + bestMove;

        movePos = GridController.Instance.GridToWorldCenter(OnGridPosition);
        
        _move = true;
    }

    private void Update() {
        if (!_move) return;

        _transform.position = Vector3.MoveTowards(_transform.position, movePos, moveSpeed * Time.deltaTime);
        
        Vector3 direction = (movePos - _transform.position).normalized;
        
        if (direction.sqrMagnitude > 0.001f) {
            Vector3 rotationAxis = Vector3.Cross(Vector3.up, direction).normalized;
            _transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.World);
        }
        
        if (Vector3.Distance(_transform.position, movePos) <= 0.001f)
            _move = false;
    }
}