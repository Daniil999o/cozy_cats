using System.Linq;
using UnityEngine;

public class RoomObjectsLoader : MonoBehaviour
{
    [field: SerializeField] public ShopItemsSO[] ShopItems { get; private set; }

    private ShopItemSO GetItemSO(string id) =>
        ShopItems.Select(x => x.GetItemSO(id)).FirstOrDefault(item => item);

    [Space]

    [SerializeField] private Transform objectsParent;
    [SerializeField] private Transform defaultSpawnPosition;

    private static RoomObjectsLoader _instance;

    public static RoomObjectsLoader Instance {
        get {
            if (!_instance)
                _instance = FindFirstObjectByType<RoomObjectsLoader>(FindObjectsInactive.Include);

            return _instance;
        }
    }

    private void Start() {
        LoadObjects();
    }

    private void LoadObjects() {
        SaveData[] datas = RoomObjectsDataPackager.DecryptDatas(RoomShop.GetSavedObjectsData());

        foreach (var x in datas) {
            ShopItemSO myItem = GetItemSO(x.ItemId);
            
            if (!myItem) continue;

            RoomObject obj = Instantiate(myItem.objectPrefab);
            
            InitializeObject(obj, x);
        }
    }

    public void SpawnObject(ShopItemSO shopItem) {
        RoomObject obj = Instantiate(shopItem.objectPrefab);

        string newName = RoomShop.CreateNewObject(obj.ElementId, obj.gameObject);

        SaveData saveData = new SaveData {
            ElementId = newName, Position = defaultSpawnPosition.position, Rotation = Vector3.zero
        };

        InitializeObject(obj, saveData);
    }

    private void InitializeObject(RoomObject obj, SaveData saveData) {
        obj.ElementId = saveData.ElementId;
        obj.transform.position = saveData.Position;
        obj.transform.rotation = Quaternion.Euler(saveData.Rotation);
        
        obj.transform.SetParent(objectsParent);

        obj.Initialize();
    }
}