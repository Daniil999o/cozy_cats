using UnityEngine;
using System.Collections.Generic;

public class StickToTerrain : MonoBehaviour
{
    [SerializeField] private bool stickInEditor;
    
    [Space]
    
    [SerializeField] private bool stickRotation = true;
    
    private Terrain _activeTerrain;
    private Transform _activeTerrainT;
    
    private Transform _transform;

    private const bool DynamicActiveTerrain = false;

    private static HashSet<StickToTerrain> _stickToTerrains = new HashSet<StickToTerrain>();
    
    private Terrain ActiveTerrain {
        set {
            _activeTerrain = value;
            _activeTerrainT = _activeTerrain.transform;
        }
    }
    
    private void Awake() {
        _transform = GetComponent<Transform>();
        ActiveTerrain = Terrain.activeTerrain;
    }

    private void OnEnable() {
        _stickToTerrains.RemoveWhere(x => !x);
        _stickToTerrains.Add(this);
    }

    private void OnDisable() {
        if (_stickToTerrains.Contains(this))
            _stickToTerrains.Remove(this);
    }

    #if UNITY_EDITOR
    
    private void OnValidate() {
        if (Application.isPlaying) return;

        if (stickInEditor) {
            stickInEditor = false;
            Awake();
            UpdateStickObject();
        }
    }
    
    #endif

    private void UpdateStickObject() {
        if (DynamicActiveTerrain && Terrain.activeTerrain != _activeTerrain)
            ActiveTerrain = Terrain.activeTerrain;

        if (!_activeTerrain) return;
        
        Vector3 terrainLocalPos = _activeTerrainT.InverseTransformPoint(_transform.position);
        
        float height = _activeTerrain.SampleHeight(terrainLocalPos);
        
        terrainLocalPos.y = height;
        _transform.position = _activeTerrainT.TransformPoint(terrainLocalPos);
        
        if (!stickRotation) return;

        TerrainData terrainData = _activeTerrain.terrainData;


        Vector3 terrainNormal = _activeTerrain.terrainData.GetInterpolatedNormal(terrainLocalPos.x / terrainData.size.x,
            terrainLocalPos.z / terrainData.size.z
        );
        
        Quaternion targetRotation = Quaternion.LookRotation(_transform.forward, terrainNormal);
        _transform.rotation = targetRotation;
    }

    public static void UpdateStickObjects() {
        foreach (var x in _stickToTerrains)
            x.UpdateStickObject();
    }
}