using System;
using UnityEngine;
using System.Collections.Generic;

namespace EasySaveLoad
{
    public class SaveLoadHandler : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool debugMode = true;

        private static SaveLoadHandler _instance;
        private readonly Dictionary<string, ISaveable> _saveables = new Dictionary<string, ISaveable>();

        public static SaveLoadHandler Instance {
            get {
                if (!_instance) _instance = FindFirstObjectByType<SaveLoadHandler>(FindObjectsInactive.Include);

                return _instance;
            }
        }
        
        public static void Register(ISaveable saveable) {
            if (saveable == null || string.IsNullOrEmpty(saveable.SaveID)) return;

            Instance._saveables[saveable.SaveID] = saveable;

            if (Instance.debugMode) Debug.Log($"Registered: {saveable.SaveID}");
        }
        
        public static void Unregister(ISaveable saveable) {
            if (saveable == null || !Instance) return;

            Instance._saveables.Remove(saveable.SaveID);
        }
        
        public string Save() {
            Dictionary<string, object> allData = new Dictionary<string, object>();

            foreach (KeyValuePair<string, ISaveable> kvp in _saveables) {
                try {
                    object data = kvp.Value.GetSaveData();
                    allData[kvp.Key] = data;
                } catch (Exception e) {
                    Debug.LogError($"Failed to save {kvp.Key}: {e.Message}");
                }
            }

            string json = JsonUtility.ToJson(new SerializableDictionary(allData));

            if (debugMode) Debug.Log($"Saved {_saveables.Count} objects. JSON: {json}");

            return json;
        }
        
        public void Load(string json) {
            if (string.IsNullOrEmpty(json)) {
                Debug.LogWarning("No save data to load");
                return;
            }

            try {
                SerializableDictionary wrapper = JsonUtility.FromJson<SerializableDictionary>(json);
                Dictionary<string, object> allData = wrapper.ToDictionary();

                int loadedCount = 0;
                foreach (KeyValuePair<string, object> kvp in allData) {
                    if (!_saveables.TryGetValue(kvp.Key, out ISaveable saveable)) continue;

                    try {
                        saveable.LoadSaveData(kvp.Value);
                        loadedCount++;
                    } catch (Exception e) {
                        Debug.LogError($"Failed to load {kvp.Key}: {e.Message}");
                    }
                }

                if (debugMode) Debug.Log($"Loaded {loadedCount} objects from save data");
            } catch (Exception e) {
                Debug.LogError($"Load failed: {e.Message}");
            }
        }
        
        [Serializable]
        private class SerializableDictionary{
            public List<string> keys = new List<string>();
            public List<string> values = new List<string>();

            public SerializableDictionary() { }

            public SerializableDictionary(Dictionary<string, object> dict) {
                foreach (KeyValuePair<string, object> kvp in dict) {
                    keys.Add(kvp.Key);
                    values.Add(JsonUtility.ToJson(kvp.Value));
                }
            }

            public Dictionary<string, object> ToDictionary() {
                Dictionary<string, object> dict = new Dictionary<string, object>();

                for (int i = 0; i < keys.Count; i++) {
                    ObjectWrapper wrapper = JsonUtility.FromJson<ObjectWrapper>(values[i]);
                    dict[keys[i]] = wrapper.value;
                }

                return dict;
            }
        }
        
        [Serializable]
        private class ObjectWrapper
        {
            public string json;
            public object value => json;

            public ObjectWrapper(object obj) => json = JsonUtility.ToJson(obj);
        }
    }
}