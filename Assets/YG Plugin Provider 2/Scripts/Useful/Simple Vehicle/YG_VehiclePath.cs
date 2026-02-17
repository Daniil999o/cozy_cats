using UnityEngine;
using System.Linq;
using System.Collections.Generic;


[System.Serializable]
public class Waypoint
{
    public Transform transform;
}

public class YG_VehiclePath : MonoBehaviour
{
    public List<Waypoint> waypoints = new List<Waypoint>();
    public bool isClosedLoop = false;
    public LayerMask surfaceLayer;

    public bool drawWhenSelected;

    public bool IsEndPoint(int point) => point >= waypoints.Count - 1;


    public int NextWaypointIndex(int currentWaypointIndex) {
        if (isClosedLoop)
            return (currentWaypointIndex + 1) % waypoints.Count;
        else
            return Mathf.Clamp(currentWaypointIndex + 1, 0, waypoints.Count - 1);
    }


#if UNITY_EDITOR
    private void OnDrawGizmos() {
        if (drawWhenSelected) return;
        
        DrawGizmos();
    }
    
    private void OnDrawGizmosSelected() {
        if (!drawWhenSelected) return;
        
        DrawGizmos();
    }

    private void DrawGizmos() {
        if (waypoints == null || waypoints.Count < 2) return;

        Gizmos.color = Color.green;
        Gizmos.DrawLineStrip(waypoints.Select(x => x.transform.position).ToArray(), isClosedLoop);
        
        for (int i = 0; i < waypoints.Count; i++) {
            Transform t = waypoints[i].transform;
            
            if (t) {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(t.position, 0.5f);
                
                Gizmos.color = Color.white;
                GizmosYG.DrawText($"{t.name}", t.position, Vector2.up * 3f + Vector2.right * 2f);
            }
        }
    }
#endif
}