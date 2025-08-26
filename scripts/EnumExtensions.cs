using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnumExtensions {

	public static bool IsAdjacent(this Vector2Int v2, Vector2Int target) {
		Vector2Int direction = v2 - target;
		return direction == Vector2Int.up || direction == Vector2Int.down || direction == Vector2Int.left || direction == Vector2Int.right;
	}
}