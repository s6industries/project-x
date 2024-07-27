namespace RaycastPro
{
    using System;
    using UnityEngine;
    using System.Collections.Generic;
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys = new List<TKey>();
        [SerializeField]
        private List<TValue> values = new List<TValue>();
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (var kvp in this)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }
        public void OnAfterDeserialize()
        {
            Clear();
            if (keys.Count != values.Count)
            {
                throw new Exception("The number of keys and values in the dictionary does not match.");
            }
            for (int i = 0; i < keys.Count; i++)
            {
                Add(keys[i], values[i]);
            }
        }
    }
    
    public class RCProSerialize
    {
        
    }
}