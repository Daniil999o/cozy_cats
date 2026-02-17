using System.Collections.Generic;
using UnityEngine;

public class CatRig : MonoBehaviour
{
    [SerializeField] private Transform root;
    [SerializeField] private Vector2 jointLimits = new Vector2(-5f, 5f);
    [SerializeField] private float bounces = 2f;
    
    private Rigidbody[] _rbs;

    private bool RigEnabled {
        set {
            foreach (var x in _rbs)
                x.isKinematic = !value;
        }
    }

    private void Awake() {
        List<Rigidbody> rbs = new List<Rigidbody>();

        foreach (var x in GetComponentsInChildren<CharacterJoint>(true)) {
            if (x.TryGetComponent(out Rigidbody rb)) rbs.Add(rb);

            SoftJointLimit limit = x.lowTwistLimit;
            limit.limit = jointLimits.x;
            limit.bounciness = bounces;

            x.lowTwistLimit = limit;

            limit = x.highTwistLimit;
            limit.limit = jointLimits.y;
            limit.bounciness = bounces;

            x.highTwistLimit = limit;
            
            limit = x.swing1Limit;
            limit.limit = jointLimits.y;
            limit.bounciness = bounces;
            
            x.swing1Limit = limit;

            limit = x.swing2Limit;
            limit.limit = jointLimits.y;
            limit.bounciness = bounces;

            x.swing2Limit = limit;
        }
        
        _rbs = rbs.ToArray();
    }

    private void Start() {
        RigEnabled = true;
    }
}