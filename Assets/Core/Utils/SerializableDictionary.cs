using System;
using System.Collections.Generic;
using UnityEngine;

namespace TWK.Utils
{
    /// <summary>
    /// Base class for serializable dictionaries in Unity.
    /// Unity cannot serialize Dictionary directly, so we use parallel lists.
    /// </summary>
    [Serializable]
    public abstract class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver, IEnumerable<KeyValuePair<TKey, TValue>>
    {
        [SerializeField]
        protected List<TKey> keys = new List<TKey>();

        [SerializeField]
        protected List<TValue> values = new List<TValue>();

        private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

        public Dictionary<TKey, TValue> Dictionary => dictionary;

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();

            foreach (var kvp in dictionary)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            dictionary = new Dictionary<TKey, TValue>();

            for (int i = 0; i < Math.Min(keys.Count, values.Count); i++)
            {
                if (!dictionary.ContainsKey(keys[i]))
                {
                    dictionary[keys[i]] = values[i];
                }
            }
        }

        // Dictionary interface methods
        public void Add(TKey key, TValue value)
        {
            dictionary.Add(key, value);
        }

        public bool Remove(TKey key)
        {
            return dictionary.Remove(key);
        }

        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get => dictionary[key];
            set => dictionary[key] = value;
        }

        public int Count => dictionary.Count;

        public void Clear()
        {
            dictionary.Clear();
        }

        public Dictionary<TKey, TValue>.KeyCollection Keys => dictionary.Keys;
        public Dictionary<TKey, TValue>.ValueCollection Values => dictionary.Values;

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        public TValue GetValueOrDefault(TKey key, TValue defaultValue = default)
        {
            return dictionary.TryGetValue(key, out TValue value) ? value : defaultValue;
        }
    }
}
