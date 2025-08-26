using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace jb5n {
	public class TilemapUtilities : MonoBehaviour {
		public static IEnumerable<Vector3Int> GetAllTilesInTilemap(Tilemap tmap) {
			BoundsInt bounds = tmap.cellBounds;
			for (int y = bounds.yMin; y < bounds.yMax; y++) {
				for (int x = bounds.xMin; x < bounds.xMax; x++) {
					Vector3Int pos = new Vector3Int(x, y);
					if (tmap.GetTile(pos) != null) {
						yield return pos;
					}
				}
			}
		}
	}
}