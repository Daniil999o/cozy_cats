using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class EventMarker : MonoBehaviour
{
    [SerializeField] private string markerName = "Marker";
    [SerializeField] private UnityEvent events;

    private static Dictionary<string, EventMarker> _eventMarkers = new Dictionary<string, EventMarker>();
    
    private void OnEnable() {
        _eventMarkers.Add(markerName, this);
    }

    private void OnDisable() {
        _eventMarkers.Remove(markerName);
    }

    private void InvokeEvents() {
        events?.Invoke();
    }

    public static void InvokeMarker(string markerName) {
        print($"{markerName}: has {_eventMarkers.ContainsKey(markerName)}");
        if (_eventMarkers.TryGetValue(markerName, out EventMarker marker))
            marker.InvokeEvents();
    }
}