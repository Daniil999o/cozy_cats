using UnityEngine;

public class TriggerTarget : MonoBehaviour
{
    [SerializeField] private string targetName = "New Target";

    public string Name => targetName;
}