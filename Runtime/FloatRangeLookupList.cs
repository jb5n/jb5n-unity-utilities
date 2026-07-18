using System.Collections;
using System.Collections.Generic;

// A list of values, each with an associated float key, that supports fast
// "give me everything within X of this value" queries.
// Multiple entries may share the same key.
//
// Entries are kept in a sorted array that is rebuilt lazily on the first query
// after a change, so queries are O(log n + k) for k results while adds stay O(1).
// Prefer adding all entries up front; interleaving Add and query calls forces a
// re-sort each time.
[System.Serializable]
public class FloatRangeLookupList<T> : IEnumerable<T> {
	[System.Serializable]
	public struct Entry {
		public T value;
		public float key;

		public Entry(T value, float key) {
			this.value = value;
			this.key = key;
		}
	}

	public List<Entry> entries = new();

	private bool isSorted = false;

	public int Count => entries.Count;

	public void Add(T value, float key) {
		entries.Add(new Entry(value, key));
		isSorted = false;
	}

	// Removes the first entry holding this value, regardless of its key.
	public bool Remove(T value) {
		int index = entries.FindIndex(e => EqualityComparer<T>.Default.Equals(e.value, value));
		if (index < 0) {
			return false;
		}
		// Removal preserves the existing order, so the sorted flag still holds.
		entries.RemoveAt(index);
		return true;
	}

	public void Clear() {
		entries.Clear();
		isSorted = true;
	}

	// Fills results with every value whose key is within distance of key (inclusive).
	// Clears results first. Values come back in ascending key order.
	// A negative distance matches nothing.
	public void GetWithinDistance(float key, float distance, List<T> results) {
		results.Clear();
		if (entries.Count == 0 || distance < 0f) {
			return;
		}

		Sort();

		float min = key - distance;
		float max = key + distance;
		for (int i = LowerBound(min); i < entries.Count && entries[i].key <= max; i++) {
			results.Add(entries[i].value);
		}
	}

	// Allocating convenience wrapper. Use the List overload in hot paths.
	public List<T> GetWithinDistance(float key, float distance) {
		List<T> results = new();
		GetWithinDistance(key, distance, results);
		return results;
	}

	// Sorts by key if anything has been added since the last sort.
	public void Sort() {
		if (isSorted) {
			return;
		}
		entries.Sort((a, b) => a.key.CompareTo(b.key));
		isSorted = true;
	}

	// Index of the first entry with a key >= value, or Count if there is none.
	// Assumes entries is sorted.
	private int LowerBound(float value) {
		int low = 0;
		int high = entries.Count;
		while (low < high) {
			int mid = low + (high - low) / 2;
			if (entries[mid].key < value) {
				low = mid + 1;
			}
			else {
				high = mid;
			}
		}
		return low;
	}

	// Enumerates values in ascending key order.
	public IEnumerator<T> GetEnumerator() {
		Sort();
		foreach (Entry entry in entries) {
			yield return entry.value;
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
