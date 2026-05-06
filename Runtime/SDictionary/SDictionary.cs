using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SKYNET {

[Serializable]
public class SPair<K, V> {
    public K key;
    public V value;
}

[Serializable]
public class SDictionary<K, V> : ISerializationCallbackReceiver, IEnumerable<KeyValuePair<K, V>>  {
    [SerializeField]
    private List<SPair<K, V>> pairs = new();

    private Dictionary<K, V> dict;

    private Dictionary<K, int> indexByKey;

    public void OnAfterDeserialize() {
        BuildDictionary();
    }

    public void OnBeforeSerialize() {}

    private void EnsureDictionary() {
        if(dict == null)
            BuildDictionary();
    }

    private void BuildDictionary() {
        dict = new Dictionary<K, V>();
        indexByKey = new Dictionary<K, int>();

        //一般不成立
        if(pairs == null) {
            pairs = new();
            return;
        }

        for(int a = 0; a < pairs.Count; ++a) {
            SPair<K, V> pair = pairs[a];
            if(pair == null || pair.key == null) continue;
            if(dict.ContainsKey(pair.key))
                Debug.LogWarning("重复的key, 后者已覆盖前者");
            dict[pair.key] = pair.value;
            indexByKey[pair.key] = a;
        }
    }

    private void RebuildIndexByKey() {
        indexByKey.Clear();

        if(pairs == null) return;

        for(int k = 0; k < pairs.Count; ++k) {
            SPair<K, V> pair = pairs[k];

            if(pair == null || pair.key == null) continue;

            indexByKey[pair.key] = k;
        }
    }

    public int Count {
        get {
            EnsureDictionary();
            return dict.Count;
        }
    }

    public V this[K key] {
        get {
            EnsureDictionary();
            return dict[key];
        }
        set {
            EnsureDictionary();

            dict[key] = value;

#if UNITY_EDITOR
            if(indexByKey.TryGetValue(key, out int index))
                pairs[index].value = value;
            else {
                int newIndex = pairs.Count;
                pairs.Add(new SPair<K, V> {
                    key = key,
                    value = value
                });
                indexByKey[key] = newIndex;
            }
#endif
        }
    }

    public bool TryGetValue(K key, out V value) {
        EnsureDictionary();
        return dict.TryGetValue(key, out value);
    }

    public bool ContainsKey(K key) {
        EnsureDictionary();
        return dict.ContainsKey(key);
    }

    public bool Remove(K key) {
        EnsureDictionary();
        bool removedFromDict = dict.Remove(key);
        bool removedFromPairs = false;

#if UNITY_EDITOR

        for(int k = pairs.Count - 1; k >= 0; --k) {
            SPair<K, V> pair = pairs[k];

            if(pair == null) continue;

            if(EqualityComparer<K>.Default.Equals(pair.key, key)) {
                pairs.RemoveAt(k);
                removedFromPairs = true;
            }
        }

        if(removedFromPairs)
            RebuildIndexByKey();
        else indexByKey.Remove(key);
#endif

        return removedFromDict || removedFromPairs;
    }

    public Dictionary<K, V>.Enumerator GetEnumerator() {
        EnsureDictionary();
        return dict.GetEnumerator();
    }

    IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator() {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}
}
