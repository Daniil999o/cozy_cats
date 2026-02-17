using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(YG_VehiclePath))]
[CanEditMultipleObjects]
public class YG_VehiclePathEditor : Editor
{
    private YG_VehiclePath path;


    private void OnEnable() {
        path = (YG_VehiclePath)target;
    }


    public override void OnInspectorGUI() {
        serializedObject.Update();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("YG_VehiclePath Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Left Ctrl + LBM to add point\n\nGizmos must be turned on", EditorStyles.helpBox);
        
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.padding = new RectOffset(10, 10, 10, 10);
        EditorGUILayout.BeginVertical(boxStyle);


        EditorGUILayout.PropertyField(serializedObject.FindProperty("drawWhenSelected"),
            new GUIContent("Draw Only When Selected", "Should the path draws in gizmos when object is selected?")
        );
        
        EditorGUILayout.Space();
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("isClosedLoop"),
            new GUIContent("Closed Loop", "Should the path loop back to the beginning?")
        );
        EditorGUILayout.PropertyField(serializedObject.FindProperty("surfaceLayer"),
            new GUIContent("Surface Layer", "The layer to raycast against when placing waypoints.")
        );

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Waypoints", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        for (int i = 0; i < path.waypoints.Count; i++) {
            EditorGUILayout.BeginHorizontal();
            path.waypoints[i].transform =
                (Transform)EditorGUILayout.ObjectField(path.waypoints[i].transform, typeof(Transform), true);

            if (GUILayout.Button("-", GUILayout.Width(20))) {
                RemoveWaypointAtIndex(i);


                break;
            }


            EditorGUILayout.EndHorizontal();
        }

        EditorGUI.indentLevel--;

        EditorGUILayout.Space();

        if (GUILayout.Button("Update All")) {
            UpdateRaycasts();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Clear All")) {
            path.waypoints = new List<Waypoint>();
        }


        EditorGUILayout.EndVertical();
        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        HandleInput();
        MovePoints();

        // Предотвращаем выделение объектов при зажатом Ctrl.
        if (Event.current.control)
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);
        }
    }

    private void HandleInput()
    {
        Event guiEvent = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);


        if (guiEvent.control && guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
        {
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, path.surfaceLayer))
            {
                AddWaypoint(hit.point);
            }
        }
    }


    private void AddWaypoint(Vector3 position) {
        Waypoint newWaypoint = new Waypoint();
        GameObject waypointObject = new GameObject("Waypoint " + path.waypoints.Count);
        waypointObject.transform.parent = path.transform;
        waypointObject.transform.position = position;
        newWaypoint.transform = waypointObject.transform;


        path.waypoints.Add(newWaypoint);
        Undo.RegisterCreatedObjectUndo(waypointObject, "Create Waypoint");
        UpdateWaypoints();
    }


    private void MovePoints() {
        for (int i = 0; i < path.waypoints.Count; i++) {
            if (path.waypoints[i].transform) {
                EditorGUI.BeginChangeCheck();
                Vector3 newTargetPosition = Handles.PositionHandle(path.waypoints[i].transform.position,
                    path.waypoints[i].transform.rotation
                );


                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(path.waypoints[i].transform, "Move Waypoint");
                    RaycastHit hit;
                    Ray ray = new Ray(newTargetPosition + Vector3.up * 100f, Vector3.down);


                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, path.surfaceLayer))
                        path.waypoints[i].transform.position = hit.point;
                    else
                        path.waypoints[i].transform.position = newTargetPosition;
                }
            }
        }
        
        UpdateWaypoints();
    }

    private void UpdateRaycasts() {
        for (int i = 0; i < path.waypoints.Count; i++) {
            if (path.waypoints[i].transform) {
                Vector3 pos = path.waypoints[i].transform.position;
                
                RaycastHit hit;
                Ray ray = new Ray(pos + Vector3.up * 100f, Vector3.down);


                if (Physics.Raycast(ray, out hit, Mathf.Infinity, path.surfaceLayer))
                    path.waypoints[i].transform.position = hit.point;
                else
                    path.waypoints[i].transform.position = pos;
            }
        }
    }

    private void RemoveWaypointAtIndex(int index) {
        if (index >= 0 && index < path.waypoints.Count) {
            Waypoint waypoint = path.waypoints[index];
            Transform transformToDelete = waypoint.transform;


            path.waypoints.RemoveAt(index);
            UpdateWaypoints();

            Undo.DestroyObjectImmediate(transformToDelete.gameObject);
        }
    }

    private void UpdateWaypoints() {
        path.waypoints.RemoveAll(x => !x.transform);
    }
}
#endif