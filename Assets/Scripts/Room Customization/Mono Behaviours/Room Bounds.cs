using UnityEngine;
using System.Collections.Generic;

public class RoomBounds : MonoBehaviour
{
    [SerializeField] private Bounds localBounds = new Bounds(Vector3.zero, Vector3.one * 10f);

    public Bounds LocalBounds => localBounds;

    public static readonly List<RoomBounds> AllRooms = new List<RoomBounds>();

    private void OnEnable() => AllRooms.Add(this);
    private void OnDisable() => AllRooms.Remove(this);


    public static Vector3 GetPositionInBounds(Vector3 worldPos, out bool changed) {
        if (AllRooms.Count == 0) {
            changed = false;
            return worldPos;
        }

        float minDistSq = float.MaxValue;
        Vector3 bestPoint = worldPos;

        foreach (RoomBounds room in AllRooms) {
            Vector3 localPos = room.transform.InverseTransformPoint(worldPos);
            Vector3 localClosest = room.localBounds.ClosestPoint(localPos);
            Vector3 worldClosest = room.transform.TransformPoint(localClosest);

            float distSq = (worldClosest - worldPos).sqrMagnitude;
            if (!(distSq < minDistSq)) continue;

            minDistSq = distSq;
            bestPoint = worldClosest;
        }

        changed = (bestPoint - worldPos).sqrMagnitude > 1e-6f;

        return bestPoint;
    }

    private void OnDrawGizmos() {
        if (!enabled) return;

        Vector3 worldCenter = transform.TransformPoint(localBounds.center);
        Vector3 worldSize = Vector3.Scale(localBounds.size, transform.lossyScale);

        Color gizmoColor = Color.mediumAquamarine;

        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(worldCenter, worldSize);
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, gizmoColor.a * 0.3f);
        Gizmos.DrawCube(worldCenter, worldSize);
    }
}