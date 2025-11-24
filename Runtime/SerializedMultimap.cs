using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[System.Serializable]
public class SerializedMultimap<TKey, TValue> : IEnumerable {
	[System.Serializable]
	public struct Entry {
		public TKey key;
		public TValue value;
	}

	[ShowInInspector]
	public List<Entry> entries = new();

	private bool isInitialized = false;
	private Dictionary<TKey, TValue> dict = new();

	public TValue this[TKey key] {
		get => GetValue(key);
		set => dict[key] = value;
	}

	public void Initialize() {
		dict.Clear();
		isInitialized = false;
		foreach (Entry entry in entries) {
			if (!dict.ContainsKey(entry.key)) {
				dict[entry.key] = entry.value;
			}
			else {
				Debug.LogError("SerializedMultimap has duplicate key: " + entry.key);
				return;
			}
		}
		isInitialized = true;
	}

	public bool GetIsInitialized() { return isInitialized; }

	public TValue GetValue(TKey key) {
		if (!isInitialized) {
			Initialize();
		}
		return dict[key];
	}

	public bool TryGetValue(TKey key, out TValue value) {
		if (!isInitialized) {
			Initialize();
		}
		return dict.TryGetValue(key, out value);
	}

	public Dictionary<TKey, TValue> GetDictionary() {
		if (!isInitialized) {
			Initialize();
		}
		return dict;
	}

	public IEnumerator GetEnumerator() {
		return GetDictionary().GetEnumerator();
	}
}
