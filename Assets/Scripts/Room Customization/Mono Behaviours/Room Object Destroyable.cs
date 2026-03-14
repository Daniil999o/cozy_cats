using UnityEngine;

public class RoomObjectDestroyable : MonoBehaviour
{
    [SerializeField] private RoomObject mainObject;

    public string Name => Id;
    private string Id => mainObject.ElementId;

    private void Awake() {
        if (!mainObject) mainObject = GetComponent<RoomObject>();
    }
    
    public void DestroyMe() {
        RoomShop.RemoveObjectData(Id);
        Destroy(gameObject);
    }

    public void DestroyRequest() => RoomUIHandler.Instance.DestroyObjectRequest(this);
}