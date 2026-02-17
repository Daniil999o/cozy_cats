using System;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace EasySaveLoad
{
    public abstract class AutoSaveable : MonoBehaviour, ISaveable
    {
        [Header("Save Settings")]
        [SerializeField] private string saveID = "";

        [SerializeField] private bool autoGenerateID = true;

        public string SaveID {
            get {
                if (autoGenerateID) saveID = $"{GetType().Name}_{gameObject.name}_{GetInstanceID()}";

                return saveID;
            }
        }

        [Serializable]
        protected class SaveDataWrapper
        {
            public List<string> keys = new List<string>();
            public List<string> values = new List<string>();
        }

#if UNITY_EDITOR

        private void OnValidate() {
            saveID = SaveID;
        }

#endif

        private void OnEnable() {
            SaveLoadHandler.Register(this);
        }

        private void OnDisable() {
            SaveLoadHandler.Unregister(this);
        }

        protected virtual void BeforeSave() { }

        public virtual object GetSaveData() {
            SaveDataWrapper wrapper = new SaveDataWrapper();
            FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance |
                                                     BindingFlags.Public |
                                                     BindingFlags.NonPublic
            );

            foreach (FieldInfo field in fields) {
                object[] attributes = field.GetCustomAttributes(typeof(SaveField), true);
                if (attributes.Length > 0) {
                    string key = field.Name;
                    SaveField saveAttr = attributes[0] as SaveField;
                    if (!string.IsNullOrEmpty(saveAttr.customKey)) key = saveAttr.customKey;

                    wrapper.keys.Add(key);

                    // Получаем значение поля
                    object value = field.GetValue(this);

                    // Создаем простой объект для сериализации
                    FieldValueWrapper valueWrapper = new FieldValueWrapper {
                        value = value
                    };
                    string json = JsonUtility.ToJson(valueWrapper);

                    // Убираем лишнее, оставляя только значение
                    if (json.Contains("\"value\":")) {
                        int start = json.IndexOf("\"value\":") + 8;
                        int end = json.LastIndexOf("}");
                        string valueJson = json.Substring(start, end - start);
                        wrapper.values.Add(valueJson);
                    } else
                        wrapper.values.Add("{}");
                }
            }

            return wrapper;
        }

        public virtual void LoadSaveData(object data) {
            if (data is SaveDataWrapper wrapper) {
                FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance |
                                                         BindingFlags.Public |
                                                         BindingFlags.NonPublic
                );

                for (int i = 0; i < wrapper.keys.Count; i++) {
                    string key = wrapper.keys[i];
                    string valueJson = wrapper.values[i];

                    foreach (FieldInfo field in fields) {
                        object[] attributes = field.GetCustomAttributes(typeof(SaveField), true);
                        if (attributes.Length > 0) {
                            string fieldKey = field.Name;
                            SaveField saveAttr = attributes[0] as SaveField;
                            if (!string.IsNullOrEmpty(saveAttr.customKey)) fieldKey = saveAttr.customKey;

                            if (fieldKey == key) {
                                try {
                                    // Создаем JSON, который может быть десериализован
                                    string fullJson = $"{{\"value\":{valueJson}}}";
                                    FieldValueWrapper valueWrapper = JsonUtility.FromJson<FieldValueWrapper>(fullJson);

                                    // Устанавливаем значение поля
                                    field.SetValue(this, valueWrapper.value);
                                } catch (Exception e) {
                                    Debug.LogWarning($"Failed to set field {field.Name}: {e.Message}");
                                }

                                break;
                            }
                        }
                    }
                }
            }
        }

        [Serializable]
        private class FieldValueWrapper
        {
            public object value;
        }
    }
}