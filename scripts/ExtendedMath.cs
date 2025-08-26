using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtendedMath : MonoBehaviour {
	/*
	// -------------------------------------------------------------------
	// NUMBERS
	// -------------------------------------------------------------------
	*/

	public static float RoundToInterval(float value, float interval) {
		return (Mathf.Round(value / interval)) * interval;
	}

	public static float FloorToInterval(float value, float interval) {
		return (Mathf.Floor(value / interval)) * interval;
	}

	public static float CeilToInterval(float value, float interval) {
		return (Mathf.Ceil(value / interval)) * interval;
	}

	/*
    // -------------------------------------------------------------------
    // NOISE
    // -------------------------------------------------------------------
    */

	public static float RidgedPerlinNoise(float x, float y) {
		float perlin = Mathf.PerlinNoise(x, y);
		return 1.0f - Mathf.Abs(perlin);
	}

	public static float ExtendedPerlinNoise(Vector2 pos, float seed, float scale) {
		return Mathf.Clamp01(Mathf.PerlinNoise((scale * pos.x) + seed, (scale * pos.y) + seed));
	}

	public static float ExtendedRidgedPerlinNoise(Vector2 pos, float seed, float scale) {
		return Mathf.Clamp01(RidgedPerlinNoise((scale * pos.x) + seed, (scale * pos.y) + seed));
	}

	/*
    // -------------------------------------------------------------------
    // LINE SEGMENTS
    // -------------------------------------------------------------------
    */

	public static bool DoLinesIntersect(Vector2 aStart, Vector2 aEnd, Vector2 bStart, Vector2 bEnd) {
		return IsCounterclockwise(aStart, bStart, bEnd) != IsCounterclockwise(aEnd, bStart, bEnd) &&
			IsCounterclockwise(aStart, aEnd, bStart) != IsCounterclockwise(aStart, aEnd, bEnd);
	}

	private static bool IsCounterclockwise(Vector2 a, Vector2 b, Vector2 c) {
		return (c.y - a.y) * (b.x - a.x) > (b.y - a.y) * (c.x - a.x);
	}

	/*
    // -------------------------------------------------------------------
    // LERPING
    // -------------------------------------------------------------------
    */

	public static Vector2 QuadraticLerp(Vector2 a, Vector2 b, Vector2 c, float t) {
		Vector2 p0 = Vector2.Lerp(a, b, t);
		Vector2 p1 = Vector2.Lerp(b, c, t);
		return Vector2.Lerp(p0, p1, t);
	}

	// Returns an array index, where lerpT = 0 will return the first, and lerpT = 1 will return the last
	public static int LerpArrayIndex(float lerpT, int arraySize) {
		if (lerpT >= 1f) {
			lerpT = 0.99f;
		}
		else if (lerpT < 0f) {
			lerpT = 0f;
		}
		return Mathf.FloorToInt(Mathf.Lerp(0, arraySize, lerpT));
	}

	public static float LerpAngleUnclamped(float a, float b, float t) {
		float delta = Mathf.Repeat((b - a), 360);
		if (delta > 180)
			delta -= 360;
		return a + delta * t;
	}

	/*
    // -------------------------------------------------------------------
    // ADJACENCY
    // -------------------------------------------------------------------
    */

	public static bool ArePointsAdjacent(Vector2Int a, Vector2Int b, bool includeDiagonal = false) {
		Vector2Int[] checkingDirections = null;
		if (includeDiagonal) {
			checkingDirections = new Vector2Int[] {
				Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
				Vector2Int.up + Vector2Int.right, Vector2Int.up + Vector2Int.left,
				Vector2Int.down + Vector2Int.right, Vector2Int.down + Vector2Int.left
			};
		}
		else {
			checkingDirections = new Vector2Int[] {
				Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
			};
		}
		foreach (Vector2Int dir in checkingDirections) {
			if (a == b + dir) {
				return true;
			}
		}
		return false;
	}

	/*
    // -------------------------------------------------------------------
    // FRACTALS
    // -------------------------------------------------------------------
    */

	public static float Mandelbrot(double real, double imag) {
		float limit = 100f;
		double zReal = real;
		double zImag = imag;

		for (float i = 0; i < limit; ++i) {
			double r2 = zReal * zReal;
			double i2 = zImag * zImag;

			if (r2 + i2 > 4.0)
				return i * 0.01f;

			zImag = 2.0 * zReal * zImag + imag;
			zReal = r2 - i2 + real;
		}
		return limit * 0.01f;
	}

	public static float JuliaFractal(Vector2 pos, Vector2 offset, Vector2 dimensions, Vector2 c, float zoom) {
		float zx = 1.5f * (pos.x - dimensions.x * 0.5f) / (0.5f * zoom * dimensions.x) + offset.x;
		float zy = 1.0f * (pos.y - dimensions.y * 0.5f) / (0.5f * zoom * dimensions.y) + offset.y;
		float i = 255f;
		float tmp;
		while (zx * zx + zy * zy < 4f && i > 1) {
			tmp = zx * zx - zy * zy + c.x;
			zy = 2.0f * zx * zy + c.y;
			zx = tmp;
			i -= 1;
		}
		return i / 255f;
	}

	/*
    // -------------------------------------------------------------------
    // VORONOI
    // -------------------------------------------------------------------
    */

	// Using brute-force, computes voronoi groups for each tile and outputs them as a dictionary <voronoiPoint, tilesInGroup>
	public static Dictionary<Vector2Int, List<Vector2Int>> VoronoiBruteForce(List<Vector2Int> allTiles, Vector2Int[] voronoiPoints) {
		Dictionary<Vector2Int, List<Vector2Int>> groupings = new Dictionary<Vector2Int, List<Vector2Int>>();
		foreach (Vector2Int tile in allTiles) {
			Vector2Int groupOwner = GetClosest(tile, voronoiPoints);
			if (!groupings.ContainsKey(groupOwner)) {
				groupings.Add(groupOwner, new List<Vector2Int>());
			}
			groupings[groupOwner].Add(tile);
		}
		return groupings;
	}

	/*
    // -------------------------------------------------------------------
    // FLOOD FILL
    // -------------------------------------------------------------------
    */

	public delegate bool Vector2IntComparisonDelegate(Vector2Int location);

	// Hexagonal assumes a pointy top hex
	public static List<Vector2Int> FloodFill(Vector2Int floodFillOrigin, Vector2IntComparisonDelegate comparisonDelegate, bool hexagonal = false) {
		Stack<Vector2Int> pointsToCheck = new Stack<Vector2Int>();
		List<Vector2Int> filledPoints = new List<Vector2Int>();
		List<Vector2Int> pointsTraversed = new List<Vector2Int>();
		pointsToCheck.Push(floodFillOrigin);

		while (pointsToCheck.Count > 0) {
			Vector2Int a = pointsToCheck.Pop();
			if (!pointsTraversed.Contains(a)) {
				pointsTraversed.Insert(0, a);
				if (comparisonDelegate(a)) {
					filledPoints.Add(a);
					if (hexagonal) {
						foreach (Vector2Int toAdd in GetHexAdjacent(a)) {
							pointsToCheck.Push(toAdd);
						}
					}
					else {
						pointsToCheck.Push(a + Vector2Int.left);
						pointsToCheck.Push(a + Vector2Int.right);
						pointsToCheck.Push(a + Vector2Int.up);
						pointsToCheck.Push(a + Vector2Int.down);
					}
				}
			}
		}
		return filledPoints;
	}

	// Returns all members of the input group that touch at least one element NOT in the input group
	public static List<Vector2Int> EdgeDetect(List<Vector2Int> inputGroup, bool hexagonal = false) {
		List<Vector2Int> outputGroup = new List<Vector2Int>();
		List<Vector2Int> pointsToCheck = new List<Vector2Int>();
		foreach (Vector2Int element in inputGroup) {
			pointsToCheck.Clear();
			if (hexagonal) {
				pointsToCheck.AddRange(GetHexAdjacent(element));
			}
			else {
				pointsToCheck.Add(element + Vector2Int.left);
				pointsToCheck.Add(element + Vector2Int.right);
				pointsToCheck.Add(element + Vector2Int.up);
				pointsToCheck.Add(element + Vector2Int.down);
			}

			bool foundOutsideGroup = false;
			foreach (Vector2Int toCheck in pointsToCheck) {
				if (!inputGroup.Contains(toCheck)) {
					foundOutsideGroup = true;
					break;
				}
			}
			if (foundOutsideGroup) {
				outputGroup.Add(element);
			}
		}
		return outputGroup;
	}

	/*
    // -------------------------------------------------------------------
    // TILES
    // -------------------------------------------------------------------
    */

	public static Vector2Int[] GetOrthogonalAdjacent(Vector2Int pos) {
		return new Vector2Int[] {
			pos + Vector2Int.up,
			pos + Vector2Int.down,
			pos + Vector2Int.left,
			pos + Vector2Int.right
		};
	}

	/*
    // -------------------------------------------------------------------
    // BITWISE
    // -------------------------------------------------------------------
    */

	public static void SetBit(ref byte aByte, int pos, bool value) {
		if (value) {
			//left-shift 1, then bitwise OR
			aByte = (byte)(aByte | (1 << pos));
		}
		else {
			//left-shift 1, then take complement, then bitwise AND
			aByte = (byte)(aByte & ~(1 << pos));
		}
	}

	public static bool GetBit(byte aByte, int pos) {
		//left-shift 1, then bitwise AND, then check for non-zero
		return ((aByte & (1 << pos)) != 0);
	}

	/*
    // -------------------------------------------------------------------
    // HEXAGONS
    // -------------------------------------------------------------------
    */

	// assumes a flat top
	public static Vector2Int[] GetHexAdjacent(Vector2Int pos) {
		if (pos.y % 2 == 0) {
			return new Vector2Int[] {
			pos + Vector2Int.left,
			pos + Vector2Int.right,
			pos + Vector2Int.up,
			pos + Vector2Int.down,
			pos + Vector2Int.left + Vector2Int.down,
			pos + Vector2Int.left + Vector2Int.up
			};
		}
		else {
			return new Vector2Int[] {
				pos + Vector2Int.left,
					pos + Vector2Int.right,
					pos + Vector2Int.up,
					pos + Vector2Int.down,
					pos + Vector2Int.right + Vector2Int.down,
					pos + Vector2Int.right + Vector2Int.up
			};
		}
	}

	public static bool AreHexesAdjacent(Vector2Int a, Vector2Int b) {
		foreach (Vector2Int adj in GetHexAdjacent(a)) {
			if (adj == b) {
				return true;
			}
		}
		return false;
	}

	public enum HexDirection {
		NORTHEAST,
		EAST,
		SOUTHEAST,
		SOUTHWEST,
		WEST,
		NORTHWEST
	}

	public static Vector2Int GetHexInDirection(Vector2Int origin, HexDirection direction) {
		switch (direction) {
			case HexDirection.EAST:
				return origin + Vector2Int.right;
			case HexDirection.WEST:
				return origin + Vector2Int.left;
			case HexDirection.NORTHEAST:
				return origin.y % 2 == 0 ? origin + Vector2Int.up : origin + Vector2Int.up + Vector2Int.right;
			case HexDirection.SOUTHEAST:
				return origin.y % 2 == 0 ? origin + Vector2Int.down : origin + Vector2Int.down + Vector2Int.right;
			case HexDirection.NORTHWEST:
				return origin.y % 2 == 0 ? origin + Vector2Int.left + Vector2Int.up : origin + Vector2Int.up;
			case HexDirection.SOUTHWEST:
				return origin.y % 2 == 0 ? origin + Vector2Int.left + Vector2Int.down : origin + Vector2Int.down;
		}
		return origin;
	}

	public static HexDirection[] GetAdjacentHexDirections(HexDirection direction) {
		HexDirection[] adjacents = new HexDirection[2];
		adjacents[0] = direction == HexDirection.NORTHEAST ? HexDirection.NORTHWEST : direction - 1;
		adjacents[1] = direction == HexDirection.NORTHWEST ? HexDirection.NORTHEAST : direction + 1;
		return adjacents;
	}

	public static bool AreHexDirectionsAdjacent(HexDirection directionA, HexDirection directionB) {
		HexDirection[] adjacents = GetAdjacentHexDirections(directionA);
		return adjacents[0] == directionB || adjacents[1] == directionB;
	}

	public static HexDirection DirectionFrom(Vector2Int origin, Vector2Int direction) {
		foreach (HexDirection dir in System.Enum.GetValues(typeof(HexDirection))) {
			if (GetHexInDirection(origin, dir) == direction) {
				return dir;
			}
		}
		Debug.LogError("Failed to find hex direction. Origin: " + origin + "\tDirection: " + direction);
		return HexDirection.EAST;
	}

	public static HexDirection DirectionFrom(Vector2Int origin, Vector2Int direction, out bool successful) {
		foreach (HexDirection dir in System.Enum.GetValues(typeof(HexDirection))) {
			if (GetHexInDirection(origin, dir) == direction) {
				successful = true;
				return dir;
			}
		}
		successful = false;
		return HexDirection.EAST;
	}

	public static HexDirection AngleToHexDirection(float angleDegrees) {
		angleDegrees = Mathf.Repeat(angleDegrees, 360f);
		if (angleDegrees < 30f) {
			return HexDirection.EAST;
		}
		else if (angleDegrees < 90f) {
			return HexDirection.NORTHEAST;
		}
		else if (angleDegrees < 150f) {
			return HexDirection.NORTHWEST;
		}
		else if (angleDegrees < 210f) {
			return HexDirection.WEST;
		}
		else if (angleDegrees < 270f) {
			return HexDirection.SOUTHWEST;
		}
		else if (angleDegrees < 330f) {
			return HexDirection.SOUTHEAST;
		}
		return HexDirection.EAST;
	}

	public static float HexDirectionToAngle(HexDirection direction) {
		List<HexDirection> counterclockwiseOrientation = new List<HexDirection>() {
			HexDirection.EAST, HexDirection.NORTHEAST, HexDirection.NORTHWEST,
			HexDirection.WEST, HexDirection.SOUTHWEST, HexDirection.SOUTHEAST
		};

		return (float)counterclockwiseOrientation.IndexOf(direction) * 60f;
	}

	public static Vector2Int HexMoveTowards(Vector2Int origin, Vector2Int destination) {
		if (origin == destination) {
			return origin;
		}

		float angle = Vector2.SignedAngle(Vector2.right, destination - origin);
		return GetHexInDirection(origin, AngleToHexDirection(angle));
	}

	// Not perfect - the outermost ring might not be added
	public static Vector2Int[] OverlapCircleHexagonal(Vector2 circlePos, float radius, Grid grid) {
		Vector2Int originTile = (Vector2Int)grid.WorldToCell(circlePos);

		Vector2IntComparisonDelegate overlapCircleDelegate = delegate (Vector2Int location) {
			Vector2 worldPos = grid.CellToWorld((Vector3Int)location);
			return (worldPos - circlePos).sqrMagnitude < radius * radius;
		};

		return FloodFill(originTile, overlapCircleDelegate, true).ToArray();
	}

	public static bool IsAdjacentToGroup(Vector2Int origin, List<Vector2Int> group) {
		foreach (Vector2Int adjacentPos in GetHexAdjacent(origin)) {
			if (group.Contains(adjacentPos)) {
				return true;
			}
		}
		return false;
	}

	public static bool IsAdjacentToGroup(Vector3Int origin, List<Vector3Int> group) {
		foreach (Vector3Int adjacentPos in GetHexAdjacent((Vector2Int)origin)) {
			if (group.Contains(adjacentPos)) {
				return true;
			}
		}
		return false;
	}

	/*
    // -------------------------------------------------------------------
    // TRIGONOMETRY
    // -------------------------------------------------------------------
    */

	// Will create a sin curve centered on 0
	public static float ExtendedSin(float xRadians, float period, float amplitude) {
		return amplitude * Mathf.Sin((xRadians * 2f * Mathf.PI) / period);
	}

	// Will create a sin curve where the bottommost value is 0
	public static Vector2 RotateAtAngle(Vector2 point, float angleRadians) {
		return new Vector2(
			Mathf.Cos(angleRadians) * point.x - Mathf.Sin(angleRadians) * point.y,
			Mathf.Sin(angleRadians) * point.x + Mathf.Cos(angleRadians) * point.y
		);
	}

	public static Vector2 RadianToVector2(float radian) {
		return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
	}

	public static float GetExtendedSinLastTrough(float xRadians, float period) {
		return FloorToInterval(xRadians - 0.75f * period, period) + 0.75f * period;
	}

	/*
    // -------------------------------------------------------------------
    // ARRAYS
    // -------------------------------------------------------------------
    */

	// Used for centering elements in an array. For example, given an odd-sized array 0 1 2 3 4 (count 5), you'd get:
	// Index (input):       0   1   2   3   4
	// Position (output):  -2  -1   0   1   2
	// For an even-sized array 0 1 2 3 (count 4):
	// Index (input):       0       1       2       3
	// Position (output): -1.5    -0.5  0  0.5     1.5
	public static float GetCenteredPosition(int index, int listCount) {
		return (float)index - ((listCount - 1) * 0.5f);
	}

	/*
    // -------------------------------------------------------------------
    // GROUP COMPARISON
    // -------------------------------------------------------------------
    */

	public delegate float HighestScoringComparisonDelegate<T>(T rootValue, T comparisonValue);

	public static T GetHighestScoring<T>(T inputValue, T[] candidates, HighestScoringComparisonDelegate<T> comparisonDelegate) {
		if (candidates == null || candidates.Length == 0) {
			return default;
		}

		float bestScore = float.MinValue;
		T bestValue = default;
		foreach (T candidate in candidates) {
			float curScore = comparisonDelegate(inputValue, candidate);
			if (curScore > bestScore) {
				bestScore = curScore;
				bestValue = candidate;
			}
		}
		return bestValue;
	}

	public static Vector2 GetClosest(Vector2 inputPoint, Vector2[] candidatePoints) {
		HighestScoringComparisonDelegate<Vector2> comparer = delegate (Vector2 inputValue, Vector2 comparison) {
			// multiply by -1 so that the closest values have the highest (least-negative) score
			return (inputValue - comparison).sqrMagnitude * -1f;
		};
		return GetHighestScoring<Vector2>(inputPoint, candidatePoints, comparer);
	}

	public static Vector2Int GetClosest(Vector2Int inputPoint, Vector2Int[] candidatePoints) {
		HighestScoringComparisonDelegate<Vector2Int> comparer = delegate (Vector2Int inputValue, Vector2Int comparison) {
			// multiply by -1 so that the closest values have the highest (least-negative) score
			return (new Vector2(inputValue.x, inputValue.y) - new Vector2(comparison.x, comparison.y)).sqrMagnitude * -1f;
		};
		return GetHighestScoring<Vector2Int>(inputPoint, candidatePoints, comparer);
	}

	/*
    // -------------------------------------------------------------------
    // GRIDS
    // -------------------------------------------------------------------
    */

	public static List<Vector2Int> GetGridPointsInRadius(Vector2Int point, float radius) {
		ExtendedMath.Vector2IntComparisonDelegate inRadiusDelegate = delegate (Vector2Int location) {
			return Vector2.Distance(point, location) <= radius;
		};
		return FloodFill(point, inRadiusDelegate, false);
	}

	/*
    // -------------------------------------------------------------------
    // RANDOM
    // -------------------------------------------------------------------
    */

	// Random function that takes in an input and returns a random output from 0-1. The function is hashed so the outputs
	// have no correlation with one another but the same input provided twice will give the same output both times.
	public static float HashRandom(float input, float seed) {
		System.Random r = new System.Random((input + seed).GetHashCode());
		return (float)r.NextDouble();
	}
}