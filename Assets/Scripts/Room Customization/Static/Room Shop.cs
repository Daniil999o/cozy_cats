using System.Linq;

public static class RoomShop
{
    
#region SKINS
    
    public static bool HasCustomSkin(string id) {
        return YG_Provider.Saves.savedSkins.Contains(id);
    }

    public static bool BuyCustomSkin(string id) {
        if (HasCustomSkin(id)) return false;

        string[] saves = YG_Provider.Saves.savedSkins;

        bool success = false;
        
        for (int i = 0; i < saves.Length; i++) {
            if (!saves[i].IsEmptyOrWhiteSpaces()) continue;
            
            saves[i] = id;
            success = true;
            break;
        }
        
        YG_Provider.Saves.savedSkins = saves;
        YG_Provider.SaveProgress();

        return success;
    }

    public static string GetObjectSkin(string objectId) {
        int objectIndex = GetObjectSavedIndex(objectId);
        
        if (objectIndex == -1) return null;
        
        string result = YG_Provider.Saves.currentObjectsSkins[objectIndex];

        return result.IsEmptyOrWhiteSpaces() ? null : result;
    }

    public static void SetObjectSkin(string objectId, string skinId) {
        int objectIndex = GetObjectSavedIndex(objectId);
        
        if (objectIndex == -1) return;
        
        YG_Provider.Saves.currentObjectsSkins[objectIndex] = skinId;
        
        YG_Provider.SaveProgress();
    }

#endregion
    
#region MATERIALS
    
    public static bool HasCustomMaterial(string id) {
        return YG_Provider.Saves.savedMaterials.Contains(id);
    }

    public static bool BuyCustomMaterial(string id) {
        if (HasCustomMaterial(id)) return false;

        string[] saves = YG_Provider.Saves.savedMaterials;

        bool success = false;
        
        for (int i = 0; i < saves.Length; i++) {
            if (!saves[i].IsEmptyOrWhiteSpaces()) continue;
            
            saves[i] = id;
            success = true;
            break;
        }
        
        YG_Provider.Saves.savedMaterials = saves;
        YG_Provider.SaveProgress();

        return success;
    }

    public static string GetObjectMaterial(string objectId) {
        int objectIndex = GetObjectSavedIndex(objectId);
        
        if (objectIndex == -1) return null;
        
        string result = YG_Provider.Saves.currentObjectsMaterials[objectIndex];

        return result.IsEmptyOrWhiteSpaces() ? null : result;
    }

    public static void SetObjectMaterial(string objectId, string materialId) {
        int objectIndex = GetObjectSavedIndex(objectId);
        
        if (objectIndex == -1) return;
        
        YG_Provider.Saves.currentObjectsMaterials[objectIndex] = materialId;
        
        YG_Provider.SaveProgress();
    }

#endregion
    
#region SAVED OBJECTS
    
    private static int GetObjectSavedIndex(string objectId) {
        string[] ids = YG_Provider.Saves.savedObjectsId;

        if (ids.Contains(objectId)) {
            return ids.ToList().IndexOf(objectId);
        }

        int index = -1;
        
        for (int i = 0; i < ids.Length; i++) {
            if (!ids[i].IsEmptyOrWhiteSpaces()) continue;
            
            ids[i] = objectId;
            index = i;
            break;
        }
        
        YG_Provider.SaveProgress();

        return index;
    }
    
#endregion
    
#region PLAYER COINS

    public static int Coins {
        get => YG_Provider.Saves.money;
        set {
            YG_Provider.Saves.money = value;
            YG_Provider.SaveProgress();
            
            RoomCurrencyInfoUI.UpdateInfos();
        }
    }
    
    public static int Gems {
        get => YG_Provider.Saves.gems;
        set {
            YG_Provider.Saves.gems = value;
            YG_Provider.SaveProgress();
            
            RoomCurrencyInfoUI.UpdateInfos();
        }
    }
    
    public static int Megas {
        get => YG_Provider.Saves.megas;
        set {
            YG_Provider.Saves.megas = value;
            YG_Provider.SaveProgress();
            
            RoomCurrencyInfoUI.UpdateInfos();
        }
    }
    
#endregion

#region MAP DATA

    public static float DayValue {
        get => YG_Provider.Saves.dayValue;
        set {
            YG_Provider.Saves.dayValue = value;
            YG_Provider.SaveProgress();
        }
    }
    
#endregion
    
#region TECHNICAL
    
    public static void ResetAll() {
        YG_Provider.Saves.money = 0;
        YG_Provider.Saves.gems = 0;
        YG_Provider.Saves.megas = 0;

        YG_Provider.Saves.savedMaterials = new string[300];
        YG_Provider.Saves.savedObjectsId = new string[100];
        YG_Provider.Saves.currentObjectsMaterials = new string[100];
        
        YG_Provider.SaveProgress();
        
        RoomCurrencyInfoUI.UpdateInfos();
    }
    
#endregion
    
}