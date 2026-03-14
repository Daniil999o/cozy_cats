using System.Linq;
using UnityEngine;

public static class RoomObjectsDataPackager
{
    private static bool IsLengthIncorrect(string[] lines) => lines.Length != 4;
    
    public static string GetData(RoomObject obj) {
        SaveData saveData = new SaveData {
            ElementId = obj.ElementId,
            ItemId = obj.ShopItemSO.id,
            Position = obj.transform.position,
            Rotation = obj.transform.eulerAngles
        };

        return saveData.ToString();
    }

    public static SaveData DecryptData(string data) {
        if (data.IsEmptyOrWhiteSpaces()) return null;
        
        string[] lines = data.SplitLines();

        if (IsLengthIncorrect(lines)) return null;
        
        Vector3? pos = lines[2].ToVector3();
        Vector3? rot = lines[3].ToVector3();
        
        if (!pos.HasValue || !rot.HasValue) return null;

        SaveData saveData = new SaveData {
            ElementId = lines[0], ItemId = lines[1], Position = pos.Value, Rotation = rot.Value
        };
        
        return saveData;
    }

    public static SaveData[] DecryptDatas(string[] datas) =>
        datas.Select(DecryptData).Where(x => x != null).ToArray();
}

public class SaveData
{
    public string ElementId { get; set; }
    public string ItemId { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }

    public override string ToString() =>
        $"{ElementId}\n{ItemId}\n{Position.ToString()}\n{Rotation.ToString()}";
}