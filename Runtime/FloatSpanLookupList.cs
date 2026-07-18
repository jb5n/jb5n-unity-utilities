using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A list of values, each covering a [min, max] span, that supports fast
// "give me everything whose span contains this value" queries.
// Spans may overlap freely and multiple entries may share the same span.
//
// Entries are kept sorted by min, alongside a running maximum of every max at or
// before each index. A query binary searches for the last span that starts at or
// before the value, then walks backwards, stopping as soon as the running maximum
// drops below the value because nothing further left can reach it. Both the sort
// and the running maximum are rebuilt lazily on the first query after a change,
// so adds stay O(1).
//
// The backwards walk is O(log n + k) for k results on typical data, where spans
// are of broadly similar length. It degrades toward O(n) only when a few very long
// spans sit to the left of many short ones, since a long span keeps the running
// maximum high and defeats the early out. Reach for an interval tree if that
// describes your data.

[System.Serializable]
public class FloatSpanLookupList<T> : IEnumerable<T> {
	[System.Serializable]
	public struct Entry {
		public T value;
		public float min;
		public float max;

		public Entry(T value, float min, float max) {
			this.value = value;
			this.min = min;
			this.max = max;
		}
	}

	public List<Entry> entries = new();

	private bool isBuilt = false;
	// maxPrefix[i] is the largest max among entries[0..i]. Used to prune the walk.
	private readonly List<float> maxPrefix = new();

	public int Count => entries.Count;

	// An entry whose min exceeds its max covers nothing and will never be returned.
	public void Add(T value, float min, float max) {
		entries.Add(new Entry(value, Mathf.Min(min, max), Mathf.Max(min, max)));
		isBuilt = false;
	}

	// Removes the first entry holding this value, regardless of its span.
	public bool Remove(T value) {
		int index = entries.FindIndex(e => EqualityComparer<T>.Default.Equals(e.value, value));
		if (index < 0) {
			return false;
		}
		// Removal preserves the sort order, but the running maximums shift.
		entries.RemoveAt(index);
		isBuilt = false;
		return true;
	}

	public void Clear() {
		entries.Clear();
		maxPrefix.Clear();
		isBuilt = true;
	}

	// Fills results with every value whose span contains value, bounds inclusive.
	// Clears results first. Values come back in ascending min order.
	public void GetContaining(float value, List<T> results) {
		results.Clear();
		if (entries.Count == 0) {
			return;
		}

		Build();

		// Walk left from the last span that starts at or before value.
		for (int i = UpperBound(value) - 1; i >= 0; i--) {
			if (maxPrefix[i] < value) {
				// No entry at or before this index reaches value, so we are done.
				break;
			}
			if (entries[i].max >= value) {
				results.Add(entries[i].value);
			}
		}

		// The walk collects right to left; hand results back in ascending min order.
		results.Reverse();
	}

	// Allocating convenience wrapper. Use the List overload in hot paths.
	public List<T> GetContaining(float value) {
		List<T> results = new();
		GetContaining(value, results);
		return results;
	}

	// Sorts by min and recomputes the running maximums if anything has changed.
	public void Build() {
		if (isBuilt) {
			return;
		}

		entries.Sort((a, b) => a.min.CompareTo(b.min));

		maxPrefix.Clear();
		if (maxPrefix.Capacity < entries.Count) {
			maxPrefix.Capacity = entries.Count;
		}
		float running = float.NegativeInfinity;
		foreach (Entry entry in entries) {
			if (entry.max > running) {
				running = entry.max;
			}
			maxPrefix.Add(running);
		}

		isBuilt = true;
	}

	// Index of the first entry with a min > value, or Count if there is none.
	// Assumes entries is sorted by min.
	private int UpperBound(float value) {
		int low = 0;
		int high = entries.Count;
		while (low < high) {
			int mid = low + (high - low) / 2;
			if (entries[mid].min <= value) {
				low = mid + 1;
			}
			else {
				high = mid;
			}
		}
		return low;
	}

	// Enumerates values in ascending min order.
	public IEnumerator<T> GetEnumerator() {
		Build();
		foreach (Entry entry in entries) {
			yield return entry.value;
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
