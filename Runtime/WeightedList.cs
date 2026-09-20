using System.Collections.Generic;
using UnityEngine;
using VInspector;

// A set of values, each with an associated weight, that supports weighted random selection.
// Entries with higher weights are proportionally more likely to be picked by GetRandom().
// Backed by a SerializedDictionary so the inspector shows a single value/weight table;
// each value can therefore appear at most once.
[System.Serializable]
public class WeightedList<T> : SerializedDictionary<T, float> {
	public void Add(T value) {
		Add(value, 1f);
	}

	// Sum of all (non-negative) weights in the list.
	public float GetTotalWeight() {
		float total = 0f;
		foreach (float weight in Values) {
			total += Mathf.Max(0f, weight);
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
		if (Count == 0) {
			value = default;
			return false;
		}

		float total = GetTotalWeight();
		if (total <= 0f) {
			// All weights are zero (or negative); fall back to a uniform pick.
			int target = Random.Range(0, Count);
			foreach (T key in Keys) {
				if (target-- == 0) {
					value = key;
					return true;
				}
			}
		}

		float roll = Random.Range(0f, total);
		T last = default;
		foreach (KeyValuePair<T, float> entry in this) {
			float weight = Mathf.Max(0f, entry.Value);
			if (roll < weight) {
				value = entry.Key;
				return true;
			}
			roll -= weight;
			last = entry.Key;
		}

		// Floating point drift; return the last entry.
		value = last;
		return true;
	}
}
