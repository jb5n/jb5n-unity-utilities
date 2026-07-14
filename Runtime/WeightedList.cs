using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A list of values, each with an associated weight, that supports weighted random selection.
// Entries with higher weights are proportionally more likely to be picked by GetRandom().
[System.Serializable]
public class WeightedList<T> : IEnumerable<T> {
	[System.Serializable]
	public struct Entry {
		public T value;
		[Min(0f)] public float weight;

		public Entry(T value, float weight) {
			this.value = value;
			this.weight = weight;
		}
	}

	public List<Entry> entries = new();

	public int Count => entries.Count;

	public void Add(T value, float weight = 1f) {
		entries.Add(new Entry(value, weight));
	}

	public bool Remove(T value) {
		int index = entries.FindIndex(e => EqualityComparer<T>.Default.Equals(e.value, value));
		if (index < 0) {
			return false;
		}
		entries.RemoveAt(index);
		return true;
	}

	public void Clear() {
		entries.Clear();
	}

	// Sum of all (non-negative) weights in the list.
	public float GetTotalWeight() {
		float total = 0f;
		foreach (Entry entry in entries) {
			total += Mathf.Max(0f, entry.weight);
		}
		return total;
	}

	// Returns a value chosen at random, weighted by each entry's weight.
	// Logs an error and returns default(T) when the list is empty.
	// Falls back to a uniform pick when every weight is zero.
	public T GetRandom() {
		if (!TryGetRandom(out T value)) {
			Debug.LogError("WeightedList.GetRandom called on an empty list.");
		}
		return value;
	}

	public bool TryGetRandom(out T value) {
		if (entries.Count == 0) {
			value = default;
			return false;
		}

		float total = GetTotalWeight();
		if (total <= 0f) {
			// All weights are zero (or negative); fall back to a uniform pick.
			value = entries[Random.Range(0, entries.Count)].value;
			return true;
		}

		float roll = Random.Range(0f, total);
		foreach (Entry entry in entries) {
			float weight = Mathf.Max(0f, entry.weight);
			if (roll < weight) {
				value = entry.value;
				return true;
			}
			roll -= weight;
		}

		// Floating point drift; return the last entry.
		value = entries[^1].value;
		return true;
	}

	public IEnumerator<T> GetEnumerator() {
		foreach (Entry entry in entries) {
			yield return entry.value;
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
