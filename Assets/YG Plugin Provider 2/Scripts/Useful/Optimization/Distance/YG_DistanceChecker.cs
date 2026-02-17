using UnityEngine;
using System.Collections.Generic;

public class YG_DistanceChecker : MonoBehaviour
{
    [SerializeField] private float defaultDisableDistance = 100f;

    public static float DefaultDisableDistance => Instance.defaultDisableDistance;
    
    private static YG_DistanceChecker _instance;

    private static YG_DistanceChecker Instance {
        get {
            if (!_instance) _instance = FindFirstObjectByType<YG_DistanceChecker>();

            return _instance;
        }
    }

    private struct HiddenObject
    {
        public float enableDistance;
        public Transform hiddenTransform;

        public HiddenObject(YG_DistanceObject distanceObject) {
            enableDistance = distanceObject.DisableDistance;
            hiddenTransform = distanceObject.transform;
        }
    }

    private HashSet<HiddenObject> _hiddenObjects = new HashSet<HiddenObject>();

    public static void HideObject(YG_DistanceObject distanceObject) {
        Instance._hiddenObjects.Add(new HiddenObject(distanceObject));
        distanceObject.gameObject.SetActive(false);
    }

    private void Update() {
        CheckHiddenObjects();
    }

    private void CheckHiddenObjects() {
        _hiddenObjects.RemoveWhere(x => {
            if (Vector3.Distance(x.hiddenTransform.position, YG_DistanceTarget.Instance.position) < x.enableDistance) {
                x.hiddenTransform.gameObject.SetActive(true);
                return true;
            }
            
            return false;
        });
    }
}