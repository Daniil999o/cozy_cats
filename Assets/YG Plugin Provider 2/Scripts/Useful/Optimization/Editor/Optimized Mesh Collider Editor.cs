#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(OptimizedMeshCollider))]
public class OptimizedMeshColliderEditor : Editor
{
    private Dictionary<int, bool> foldoutStates = new Dictionary<int, bool>();
    private Collider hoveredInList = null;
    private Collider hoveredInScene = null;

    private UnityEditor.IMGUI.Controls.BoxBoundsHandle m_BoxHandle = new UnityEditor.IMGUI.Controls.BoxBoundsHandle();

    private UnityEditor.IMGUI.Controls.SphereBoundsHandle m_SphereHandle =
        new UnityEditor.IMGUI.Controls.SphereBoundsHandle();

    private UnityEditor.IMGUI.Controls.CapsuleBoundsHandle m_CapsuleHandle =
        new UnityEditor.IMGUI.Controls.CapsuleBoundsHandle();

    public override void OnInspectorGUI() {
        OptimizedMeshCollider script = (OptimizedMeshCollider)target;
        Collider[] colliders = script.GetComponents<Collider>();

        GUILayout.Label("Generation Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("minSize"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("duplicateThreshold"));
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(10);

        bool hasColliders = colliders.Length > 0;

        if (hasColliders) {
            if (GUILayout.Button("Optimize", GUILayout.Height(30))) {
                script.gameObject.AddComponent<OptimizeCollider>();
                GUIUtility.ExitGUI();
            }

            if (GUILayout.Button("Regenerate", GUILayout.Height(30))) {
                script.GenerateAndFinish();
                GUIUtility.ExitGUI();
            }

            if (GUILayout.Button("Clean All", GUILayout.Height(30))) {
                script.CleanColliders();
                GUIUtility.ExitGUI();
            }

            if (GUILayout.Button("Convert To One MeshCollider", GUILayout.Height(30))) {
                ConvertToOneMeshCollider(script);
                GUIUtility.ExitGUI();
            }
        } else {
            if (GUILayout.Button("Generate", GUILayout.Height(30))) {
                script.GenerateAndFinish();
                GUIUtility.ExitGUI();
            }
        }

        EditorGUILayout.Space(20);
        GUILayout.Label($"Collider Editor (Total: {colliders.Length})", EditorStyles.boldLabel);

        if (hasColliders)
            DrawColliderList(script, colliders);
        else
            EditorGUILayout.HelpBox("No colliders generated yet.", MessageType.Info);
    }

    private void ConvertToOneMeshCollider(OptimizedMeshCollider script) {
        GameObject obj = script.gameObject;
        BoxCollider[] boxes = obj.GetComponents<BoxCollider>();

        if (boxes.Length == 0) {
            Debug.LogWarning("No BoxColliders to convert.");
            return;
        }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Convert to Single MeshCollider");
        int undoGroup = Undo.GetCurrentGroup();

        GameObject tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh cubeMesh = tempCube.GetComponent<MeshFilter>().sharedMesh;

        CombineInstance[] combine = new CombineInstance[boxes.Length];

        for (int i = 0; i < boxes.Length; i++) {
            combine[i].mesh = cubeMesh;
            combine[i].transform = Matrix4x4.TRS(boxes[i].center, Quaternion.identity, boxes[i].size);
        }

        DestroyImmediate(tempCube);

        Mesh combinedMesh = new Mesh();
        combinedMesh.name = "CombinedCollider";
        combinedMesh.CombineMeshes(combine, true, true);

        MeshCollider meshCollider = Undo.AddComponent<MeshCollider>(obj);
        meshCollider.sharedMesh = combinedMesh;

        meshCollider.convex = false;

        foreach (Collider col in obj.GetComponents<Collider>())
            if (col != meshCollider)
                Undo.DestroyObjectImmediate(col);

        Undo.DestroyObjectImmediate(script);

        Undo.CollapseUndoOperations(undoGroup);
        Debug.Log("Converted to single MeshCollider and removed generator script.");
    }

    private void DrawColliderList(OptimizedMeshCollider script, Collider[] colliders) {
        Collider currentFrameHover = null;

        for (int i = 0; i < colliders.Length; i++) {
            Collider col = colliders[i];
            if (col == null) continue;

            if (!foldoutStates.ContainsKey(col.GetInstanceID())) foldoutStates[col.GetInstanceID()] = false;

            bool isHighlighted = col == hoveredInScene || col == hoveredInList;

            GUI.backgroundColor = isHighlighted ? new Color(0.6f, 0.8f, 1f) : Color.white;

            Rect rect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (rect.Contains(Event.current.mousePosition)) currentFrameHover = col;

            GUI.backgroundColor = Color.white;

            EditorGUILayout.BeginHorizontal();

            foldoutStates[col.GetInstanceID()] = EditorGUILayout.Foldout(foldoutStates[col.GetInstanceID()],
                $"Collider #{i + 1} ({col.GetType().Name})", true
            );

            if (GUILayout.Button("Delete", GUILayout.Width(60))) {
                Undo.DestroyObjectImmediate(col);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUIUtility.ExitGUI();
                return;
            }

            EditorGUILayout.EndHorizontal();

            if (foldoutStates[col.GetInstanceID()]) {
                EditorGUI.indentLevel++;
                DrawColliderProperties(col);
                DrawConversionButtons(script.gameObject, col);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        if (currentFrameHover != hoveredInList) {
            hoveredInList = currentFrameHover;
            SceneView.RepaintAll();
            Repaint();
        }
    }

    private void DrawColliderProperties(Collider col) {
        Undo.RecordObject(col, "Edit Collider");

        if (col is BoxCollider box) {
            box.center = EditorGUILayout.Vector3Field("Center", box.center);
            box.size = EditorGUILayout.Vector3Field("Size", box.size);
        } else if (col is SphereCollider sphere) {
            sphere.center = EditorGUILayout.Vector3Field("Center", sphere.center);
            sphere.radius = EditorGUILayout.FloatField("Radius", sphere.radius);
        } else if (col is CapsuleCollider capsule) {
            capsule.center = EditorGUILayout.Vector3Field("Center", capsule.center);
            capsule.height = EditorGUILayout.FloatField("Height", capsule.height);
            capsule.radius = EditorGUILayout.FloatField("Radius", capsule.radius);
            capsule.direction = EditorGUILayout.IntPopup("Direction", capsule.direction, new string[] {
                    "X-Axis", "Y-Axis", "Z-Axis"
                }, new int[] {
                    0, 1, 2
                }
            );
        }
    }

    private void DrawConversionButtons(GameObject obj, Collider col) {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Convert to:", EditorStyles.miniLabel);
        EditorGUILayout.BeginHorizontal();

        if (!(col is BoxCollider) && GUILayout.Button("Box")) ConvertCollider<BoxCollider>(obj, col);
        if (!(col is SphereCollider) && GUILayout.Button("Sphere")) ConvertCollider<SphereCollider>(obj, col);
        if (!(col is CapsuleCollider) && GUILayout.Button("Capsule")) ConvertCollider<CapsuleCollider>(obj, col);

        EditorGUILayout.EndHorizontal();
    }

    private void ConvertCollider<T>(GameObject obj, Collider oldCol) where T : Collider {
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Convert Collider");

        Bounds bounds = oldCol.bounds;
        Vector3 localCenter = oldCol.transform.InverseTransformPoint(bounds.center);
        Vector3 localSize = oldCol.transform.InverseTransformVector(bounds.size);
        Vector3 absSize = new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z));

        Undo.DestroyObjectImmediate(oldCol);
        T newCol = Undo.AddComponent<T>(obj);

        if (newCol is BoxCollider box) {
            box.center = localCenter;
            box.size = absSize;
        } else if (newCol is SphereCollider sphere) {
            sphere.center = localCenter;
            sphere.radius = Mathf.Max(absSize.x, absSize.y, absSize.z) * 0.5f;
        } else if (newCol is CapsuleCollider capsule) {
            capsule.center = localCenter;

            int direction = 1; // Y default
            float maxAxis = absSize.y;

            if (absSize.x > absSize.y && absSize.x > absSize.z) {
                direction = 0; // X
                maxAxis = absSize.x;
            } else if (absSize.z > absSize.x && absSize.z > absSize.y) {
                direction = 2; // Z
                maxAxis = absSize.z;
            }

            capsule.direction = direction;
            capsule.height = maxAxis;

            float otherMax = 0f;
            if (direction == 0) otherMax = Mathf.Max(absSize.y, absSize.z);
            if (direction == 1) otherMax = Mathf.Max(absSize.x, absSize.z);
            if (direction == 2) otherMax = Mathf.Max(absSize.x, absSize.y);

            capsule.radius = otherMax * 0.5f;
        }

        Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        GUIUtility.ExitGUI();
    }

    private void OnSceneGUI() {
        OptimizedMeshCollider script = (OptimizedMeshCollider)target;
        Collider[] colliders = script.GetComponents<Collider>();
        
        Collider targetCol = hoveredInList != null ? hoveredInList : hoveredInScene;

        if (targetCol == null) {
            Ray hoverRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            float minDist = float.MaxValue;
            foreach (Collider col in colliders) {
                if (col != null && col.Raycast(hoverRay, out RaycastHit hit, 1000f)) {
                    if (hit.distance < minDist) {
                        minDist = hit.distance;
                        targetCol = col;
                    }
                }
            }
        }

        bool handlesUsedEvent = false;

        if ((targetCol != null && hoveredInList != null) || (targetCol != null && hoveredInScene == targetCol)) {
            Vector3 worldCenter = Vector3.zero;
            Quaternion worldRotation = Tools.pivotRotation == PivotRotation.Local
                ? targetCol.transform.rotation
                : Quaternion.identity;

            if (targetCol is BoxCollider b)
                worldCenter = targetCol.transform.TransformPoint(b.center);
            else if (targetCol is SphereCollider s)
                worldCenter = targetCol.transform.TransformPoint(s.center);
            else if (targetCol is CapsuleCollider c) worldCenter = targetCol.transform.TransformPoint(c.center);

            EditorGUI.BeginChangeCheck();
            
            Vector3 newWorldCenter = Handles.PositionHandle(worldCenter, worldRotation);

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(targetCol, "Move Collider Center");
                Vector3 newLocalCenter = targetCol.transform.InverseTransformPoint(newWorldCenter);

                if (targetCol is BoxCollider box)
                    box.center = newLocalCenter;
                else if (targetCol is SphereCollider sphere)
                    sphere.center = newLocalCenter;
                else if (targetCol is CapsuleCollider capsule) capsule.center = newLocalCenter;

                handlesUsedEvent = true;
            }

            Matrix4x4 originalMatrix = Handles.matrix;
            Handles.matrix = targetCol.transform.localToWorldMatrix;
            Handles.color = new Color(0.0f, 0.0f, 1f, 1f);

            EditorGUI.BeginChangeCheck();

            if (targetCol is BoxCollider boxH) {
                m_BoxHandle.center = boxH.center;
                m_BoxHandle.size = boxH.size;
                m_BoxHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(boxH, "Resize Box Collider");
                    boxH.center = m_BoxHandle.center;
                    boxH.size = m_BoxHandle.size;
                    handlesUsedEvent = true;
                }
            } else if (targetCol is SphereCollider sphereH) {
                m_SphereHandle.center = sphereH.center;
                m_SphereHandle.radius = sphereH.radius;
                m_SphereHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(sphereH, "Resize Sphere Collider");
                    sphereH.center = m_SphereHandle.center;
                    sphereH.radius = m_SphereHandle.radius;
                    handlesUsedEvent = true;
                }
            } else if (targetCol is CapsuleCollider capsuleH) {
                m_CapsuleHandle.center = capsuleH.center;
                m_CapsuleHandle.height = capsuleH.height;
                m_CapsuleHandle.radius = capsuleH.radius;
                switch (capsuleH.direction) {
                    case 0:
                        m_CapsuleHandle.heightAxis = UnityEditor.IMGUI.Controls.CapsuleBoundsHandle.HeightAxis.X;
                        break;
                    case 1:
                        m_CapsuleHandle.heightAxis = UnityEditor.IMGUI.Controls.CapsuleBoundsHandle.HeightAxis.Y;
                        break;
                    case 2:
                        m_CapsuleHandle.heightAxis = UnityEditor.IMGUI.Controls.CapsuleBoundsHandle.HeightAxis.Z;
                        break;
                }

                m_CapsuleHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(capsuleH, "Resize Capsule Collider");
                    capsuleH.center = m_CapsuleHandle.center;
                    capsuleH.height = m_CapsuleHandle.height;
                    capsuleH.radius = m_CapsuleHandle.radius;
                    handlesUsedEvent = true;
                }
            }

            Handles.matrix = originalMatrix;
        }

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !Event.current.control && !Event.current.alt && GUIUtility.hotControl == 0 &&
            !handlesUsedEvent) {
            Collider closest = null;
            float minDst = float.MaxValue;
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            foreach (Collider col in colliders) {
                if (col == null) continue;
                if (col.Raycast(ray, out RaycastHit hit, 1000f)) {
                    if (hit.distance < minDst) {
                        minDst = hit.distance;
                        closest = col;
                    }
                }
            }

            if (closest != null) {
                if (!foldoutStates.ContainsKey(closest.GetInstanceID())) foldoutStates[closest.GetInstanceID()] = false;

                hoveredInScene = closest;
                hoveredInList = null;
                foldoutStates[closest.GetInstanceID()] = true;

                Event.current.Use();
                Repaint();
            }
        }
    }
}

#endif