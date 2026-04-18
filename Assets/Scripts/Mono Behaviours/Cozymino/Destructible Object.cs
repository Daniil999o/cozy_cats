using UnityEngine;
using CozyPolyomino;
using System.Linq;
using System.Collections.Generic;

public class DestructibleObject : BonusCell
{
    [Space]
    
    [SerializeField] private Animator animator;

    private bool _broken;

    private void Start() {
        blockDraggables = true;
    }

    public override void NotifyGridChanged(Vector2Int[] draggablePositions, Vector2Int[] obstaclePositions) {
        if (_broken) return;
        
        HashSet<Vector2Int> draggableSet = new HashSet<Vector2Int>(draggablePositions);
        
        Vector2Int[] directions = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        foreach (Vector2Int dir in directions) {
            Vector2Int neighbour = OnGridPosition + dir;
            
            print($"Dir: {dir}, neighbour: {neighbour}");
            
            if (!draggableSet.Contains(neighbour)) continue;
            
            print($"Contains");
            
            DraggableShape[] draggables = GridController.Instance.GetDraggablesOnCell(neighbour);
            if (draggables == null) continue;
            
            print($"Draggables: {draggables[0]}");
            
            if (!draggables.Any(ds => ds && ds.isBig)) continue;
            
            BreakMe();
            break;
        }
    }

    private void BreakMe() {
        if (_broken) return;

        _broken = true;
        
        animator.SetTrigger("Break");
        blockDraggables = false;
    }
}