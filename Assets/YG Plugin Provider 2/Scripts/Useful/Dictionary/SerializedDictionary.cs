using System.Linq;
using System.Collections.Generic;

namespace DictionaryExtension
{
    [System.Serializable]
    public struct SerializedDictionary<TKey, TValue>
    {
        [System.Serializable]
        public struct Item<TKey, TValue>
        {
            public TKey key;
            public TValue value;
        }

        public Item<TKey, TValue>[] items;

        public bool HasKey(TKey key) => items.Count(x => Compare(key, x.key)) > 0;

        public TValue GetKey(TKey key) {
            var selectedItems = items.Where(x => Compare(key, x.key)).ToArray();

            if (selectedItems.Length > 0) return selectedItems.First().value;

            return default;
        }

        public bool HasValue(TValue value) => items.Count(x => Compare(value, x.value)) > 0;

        public void RemoveKeys(TKey key) {
            if (!HasKey(key)) return;

            items = items.Where(x => !Compare(key, x.key)).ToArray();
        }

        public void Add(TKey key, TValue value) {
            var newItems = items.ToList();
            
            newItems.Add(new Item<TKey, TValue> {
                key = key,
                value = value
            });

            items = newItems.ToArray();
        }

        private static bool Compare<T>(T x, T y) => EqualityComparer<T>.Default.Equals(x, y);
    }
}