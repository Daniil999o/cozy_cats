#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[DisallowMultipleComponent]
public class OptimizedMeshCollider : MonoBehaviour
{
    public float minSize = 0.5f;
    public float duplicateThreshold = 0.5f;

    private void OnDrawGizmosSelected() {
        Color gizmosColor = new Color(0f, 1f, 0.9f, 1f);
        Gizmos.color = gizmosColor;

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        foreach (Collider x in GetComponents<Collider>()) {
            if (x is BoxCollider box) {
                Gizmos.DrawCube(box.center, box.size);
                Gizmos.DrawWireCube(box.center, box.size);
            } else if (x is SphereCollider sphere) {
                Gizmos.DrawSphere(sphere.center, sphere.radius);
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            } else if (x is CapsuleCollider capsule) {
                Quaternion rot = Quaternion.identity;
                if (capsule.direction == 0) rot = Quaternion.Euler(0, 0, 90);
                if (capsule.direction == 2) rot = Quaternion.Euler(90, 0, 0);

                Matrix4x4 capsuleMatrix = Gizmos.matrix * Matrix4x4.TRS(capsule.center, rot, Vector3.one);
                Matrix4x4 savedMtx = Gizmos.matrix;
                Gizmos.matrix = capsuleMatrix;

                GizmosYG.DrawCapsule(capsule.radius, capsule.height);
                GizmosYG.DrawWireCapsule(capsule.radius, capsule.height);

                Gizmos.matrix = savedMtx;
            }
        }

        Gizmos.matrix = oldMatrix;
    }

    public void GenerateAndFinish() {
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Generate Mesh Colliders");
        int undoGroup = Undo.GetCurrentGroup();

        foreach (Collider x in GetComponents<Collider>()) DestroyImmediate(x);

        GenerateColliders();

        Type optType = Type.GetType("OptimizeCollider");
        if (optType != null)
            if (GetComponent(optType) == null)
                Undo.AddComponent(gameObject, optType);

        Undo.CollapseUndoOperations(undoGroup);
    }

    public void CleanColliders() {
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Clean Mesh Colliders");

        int undoGroup = Undo.GetCurrentGroup();

        Collider[] colliders = GetComponents<Collider>();

        Undo.RegisterCompleteObjectUndo(gameObject, "Clean All Colliders");

        foreach (Collider x in colliders) Undo.DestroyObjectImmediate(x);

        Undo.CollapseUndoOperations(undoGroup);
    }

    public void GenerateColliders() {
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Generate Mesh Colliders");
        int undoGroup = Undo.GetCurrentGroup();

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null) {
            Debug.LogError("Object does not contains MeshFilter!");
            return;
        }

        Undo.RegisterCompleteObjectUndo(gameObject, "Clear Old Colliders");

        Collider[] oldColliders = GetComponents<Collider>();
        foreach (Collider col in oldColliders) Undo.DestroyObjectImmediate(col);

        Mesh mesh = meshFilter.sharedMesh;

        List<List<int>> subMeshesIndices = GetConnectedComponents(mesh);

        List<Bounds> candidates = new List<Bounds>();
        Vector3[] vertices = mesh.vertices;

        foreach (List<int> indices in subMeshesIndices) {
            if (indices.Count < 3) continue;

            Vector3 min = vertices[indices[0]];
            Vector3 max = vertices[indices[0]];

            foreach (int i in indices) {
                Vector3 v = vertices[i];
                min = Vector3.Min(min, v);
                max = Vector3.Max(max, v);
            }

            Bounds b = new Bounds();
            b.SetMinMax(min, max);

            Vector3 size = b.size;
            if (size.x < 0.001f) size.x = 0.01f;
            if (size.y < 0.001f) size.y = 0.01f;
            if (size.z < 0.001f) size.z = 0.01f;
            b.size = size;

            float maxDimension = Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z));

            if (maxDimension >= minSize) candidates.Add(b);
        }

        candidates = OptimizeBounds(candidates);

        foreach (Bounds bounds in candidates) {
            BoxCollider box = Undo.AddComponent<BoxCollider>(gameObject);
            box.center = bounds.center;
            box.size = bounds.size;
        }

        Debug.Log($"Done. Generated {candidates.Count} colliders.");
        Undo.CollapseUndoOperations(undoGroup);
    }

    private List<Bounds> OptimizeBounds(List<Bounds> inputBounds) {
        inputBounds.Sort((a, b) => b.size.sqrMagnitude.CompareTo(a.size.sqrMagnitude));

        List<Bounds> result = new List<Bounds>(inputBounds);

        for (int i = 0; i < result.Count; i++) {
            for (int j = result.Count - 1; j > i; j--) {
                Bounds b1 = result[i];
                Bounds b2 = result[j];

                if (b1.Contains(b2.min) && b1.Contains(b2.max)) {
                    result.RemoveAt(j);
                    continue;
                }

                if (b1.Intersects(b2)) {
                    float distCenter = Vector3.SqrMagnitude(b1.center - b2.center);
                    float distSize = Vector3.SqrMagnitude(b1.size - b2.size);
                    float thresholdSqr = duplicateThreshold * duplicateThreshold;

                    if (distCenter < thresholdSqr && distSize < thresholdSqr) result.RemoveAt(j);
                }
            }
        }

        return result;
    }

    private List<List<int>> GetConnectedComponents(Mesh mesh) {
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        int vertexCount = mesh.vertexCount;

        int[] remappedIndices = new int[vertexCount];
        Dictionary<Vector3, int> positionToId = new Dictionary<Vector3, int>();

        for (int i = 0; i < vertexCount; i++) {
            Vector3 pos = vertices[i];
            if (!positionToId.TryGetValue(pos, out int id)) {
                id = i;
                positionToId[pos] = id;
            }

            remappedIndices[i] = id;
        }

        Dictionary<int, List<int>> weldedVertexToTriangles = new Dictionary<int, List<int>>();

        for (int i = 0; i < triangles.Length; i += 3) {
            int tIndex = i / 3;

            int v1 = triangles[i];
            int v2 = triangles[i + 1];
            int v3 = triangles[i + 2];

            int w1 = remappedIndices[v1];
            int w2 = remappedIndices[v2];
            int w3 = remappedIndices[v3];

            if (!weldedVertexToTriangles.ContainsKey(w1)) weldedVertexToTriangles[w1] = new List<int>();
            if (!weldedVertexToTriangles.ContainsKey(w2)) weldedVertexToTriangles[w2] = new List<int>();
            if (!weldedVertexToTriangles.ContainsKey(w3)) weldedVertexToTriangles[w3] = new List<int>();

            weldedVertexToTriangles[w1].Add(tIndex);
            weldedVertexToTriangles[w2].Add(tIndex);
            weldedVertexToTriangles[w3].Add(tIndex);
        }

        List<List<int>> islands = new List<List<int>>();
        bool[] visitedTriangles = new bool[triangles.Length / 3];

        for (int i = 0; i < visitedTriangles.Length; i++) {
            if (visitedTriangles[i]) continue;

            HashSet<int> islandVertices = new HashSet<int>();
            Queue<int> trianglesQueue = new Queue<int>();

            trianglesQueue.Enqueue(i);
            visitedTriangles[i] = true;

            while (trianglesQueue.Count > 0) {
                int tIndex = trianglesQueue.Dequeue();

                int v1 = triangles[tIndex * 3];
                int v2 = triangles[tIndex * 3 + 1];
                int v3 = triangles[tIndex * 3 + 2];

                islandVertices.Add(v1);
                islandVertices.Add(v2);
                islandVertices.Add(v3);

                CheckNeighbors(remappedIndices[v1], weldedVertexToTriangles, visitedTriangles, trianglesQueue);
                CheckNeighbors(remappedIndices[v2], weldedVertexToTriangles, visitedTriangles, trianglesQueue);
                CheckNeighbors(remappedIndices[v3], weldedVertexToTriangles, visitedTriangles, trianglesQueue);
            }

            islands.Add(islandVertices.ToList());
        }

        return islands;
    }

    private void CheckNeighbors(int weldedVertexIndex,
        Dictionary<int, List<int>> map,
        bool[] visited,
        Queue<int> queue) {
        if (!map.ContainsKey(weldedVertexIndex)) return;

        foreach (int tIndex in map[weldedVertexIndex]) {
            if (!visited[tIndex]) {
                visited[tIndex] = true;
                queue.Enqueue(tIndex);
            }
        }
    }
}

#endif