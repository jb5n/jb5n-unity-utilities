using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorExtensions {
	public static Color CLEAR_WHITE = new Color(1f, 1f, 1f, 0f);

	public static Color MoveTowards(this Color current, Color target, float maxChangeDelta) {
		Color blend = current;
		blend.r = Mathf.MoveTowards(blend.r, target.r, maxChangeDelta);
		blend.g = Mathf.MoveTowards(blend.g, target.g, maxChangeDelta);
		blend.b = Mathf.MoveTowards(blend.b, target.b, maxChangeDelta);
		blend.a = Mathf.MoveTowards(blend.a, target.a, maxChangeDelta);
		return blend;
	}
}