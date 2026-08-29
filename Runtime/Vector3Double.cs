using System;
using UnityEngine;

namespace jb5n {
	/// <summary>
	/// Represents a 3D vector using double precision floating point values.
	/// </summary>
	[Serializable]
	public struct Vector3Double : IEquatable<Vector3Double> {
		public double x;
		public double y;
		public double z;

		/// <summary>
		/// Initializes a new instance of the Vector3Double struct.
		/// </summary>
		public Vector3Double(double x, double y, double z) {
			this.x = x;
			this.y = y;
			this.z = z;
		}

		/// <summary>
		/// Initializes a new instance of the Vector3Double struct with x and y values.
		/// </summary>
		public Vector3Double(double x, double y) {
			this.x = x;
			this.y = y;
			this.z = 0;
		}

		/// <summary>
		/// Returns the length (magnitude) of this vector.
		/// </summary>
		public double magnitude => Math.Sqrt(x * x + y * y + z * z);

		/// <summary>
		/// Returns the squared length (magnitude) of this vector.
		/// </summary>
		public double sqrMagnitude => x * x + y * y + z * z;

		/// <summary>
		/// Returns a normalized copy of this vector.
		/// </summary>
		public Vector3Double normalized {
			get {
				double mag = magnitude;
				if (mag > 0)
					return new Vector3Double(x / mag, y / mag, z / mag);
				return Vector3Double.zero;
			}
		}

		/// <summary>
		/// Normalizes this vector in place.
		/// </summary>
		public void Normalize() {
			double mag = magnitude;
			if (mag > 0) {
				x /= mag;
				y /= mag;
				z /= mag;
			}
			else {
				x = y = z = 0;
			}
		}

		/// <summary>
		/// Returns the dot product of two vectors.
		/// </summary>
		public static double Dot(Vector3Double a, Vector3Double b) {
			return a.x * b.x + a.y * b.y + a.z * b.z;
		}

		/// <summary>
		/// Returns the cross product of two vectors.
		/// </summary>
		public static Vector3Double Cross(Vector3Double a, Vector3Double b) {
			return new Vector3Double(
				a.y * b.z - a.z * b.y,
				a.z * b.x - a.x * b.z,
				a.x * b.y - a.y * b.x
			);
		}

		/// <summary>
		/// Returns the distance between two vectors.
		/// </summary>
		public static double Distance(Vector3Double a, Vector3Double b) {
			return (a - b).magnitude;
		}

		/// <summary>
		/// Returns the squared distance between two vectors.
		/// </summary>
		public static double SqrDistance(Vector3Double a, Vector3Double b) {
			return (a - b).sqrMagnitude;
		}

		/// <summary>
		/// Linearly interpolates between two vectors.
		/// </summary>
		public static Vector3Double Lerp(Vector3Double a, Vector3Double b, double t) {
			t = Math.Max(0, Math.Min(1, t));
			return new Vector3Double(
				a.x + (b.x - a.x) * t,
				a.y + (b.y - a.y) * t,
				a.z + (b.z - a.z) * t
			);
		}

		/// <summary>
		/// Linearly interpolates between two vectors without clamping.
		/// </summary>
		public static Vector3Double LerpUnclamped(Vector3Double a, Vector3Double b, double t) {
			return new Vector3Double(
				a.x + (b.x - a.x) * t,
				a.y + (b.y - a.y) * t,
				a.z + (b.z - a.z) * t
			);
		}

		/// <summary>
		/// Converts this Vector3Double to a Vector3.
		/// </summary>
		public Vector3 ToVector3() {
			return new Vector3((float)x, (float)y, (float)z);
		}

		/// <summary>
		/// Creates a Vector3Double from a Vector3.
		/// </summary>
		public static Vector3Double FromVector3(Vector3 v) {
			return new Vector3Double(v.x, v.y, v.z);
		}

		public override bool Equals(object obj) {
			if (!(obj is Vector3Double))
				return false;
			return Equals((Vector3Double)obj);
		}

		public bool Equals(Vector3Double other) {
			return x == other.x && y == other.y && z == other.z;
		}

		public override int GetHashCode() {
			return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2);
		}

		public override string ToString() {
			return $"({x}, {y}, {z})";
		}

		public string ToString(string format) {
			return $"({x.ToString(format)}, {y.ToString(format)}, {z.ToString(format)})";
		}

		// Operators
		public static Vector3Double operator +(Vector3Double a, Vector3Double b) {
			return new Vector3Double(a.x + b.x, a.y + b.y, a.z + b.z);
		}

		public static Vector3Double operator -(Vector3Double a, Vector3Double b) {
			return new Vector3Double(a.x - b.x, a.y - b.y, a.z - b.z);
		}

		public static Vector3Double operator -(Vector3Double a) {
			return new Vector3Double(-a.x, -a.y, -a.z);
		}

		public static Vector3Double operator *(Vector3Double a, double d) {
			return new Vector3Double(a.x * d, a.y * d, a.z * d);
		}

		public static Vector3Double operator *(double d, Vector3Double a) {
			return new Vector3Double(a.x * d, a.y * d, a.z * d);
		}

		public static Vector3Double operator /(Vector3Double a, double d) {
			return new Vector3Double(a.x / d, a.y / d, a.z / d);
		}

		public static bool operator ==(Vector3Double a, Vector3Double b) {
			return a.Equals(b);
		}

		public static bool operator !=(Vector3Double a, Vector3Double b) {
			return !a.Equals(b);
		}

		// Common vector constants
		public static Vector3Double zero => new Vector3Double(0, 0, 0);
		public static Vector3Double one => new Vector3Double(1, 1, 1);
		public static Vector3Double right => new Vector3Double(1, 0, 0);
		public static Vector3Double left => new Vector3Double(-1, 0, 0);
		public static Vector3Double up => new Vector3Double(0, 1, 0);
		public static Vector3Double down => new Vector3Double(0, -1, 0);
		public static Vector3Double forward => new Vector3Double(0, 0, 1);
		public static Vector3Double back => new Vector3Double(0, 0, -1);
	}
}