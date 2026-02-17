using UnityEngine;
using System.Collections.Generic;

public class YG_ComponentsController : MonoBehaviour
{
    [SerializeField] private bool startEnabled;
    [SerializeField] private List<Behaviour> behaviours;

    private void Start() {
        SetEnabled(startEnabled);
    }

    public void SetEnabled(bool value) => behaviours.ForEach(x => x.enabled = value);
}