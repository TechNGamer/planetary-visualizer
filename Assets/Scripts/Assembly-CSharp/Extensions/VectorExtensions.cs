using UnityEngine;

namespace SphereVisualizer.Extensions {
	public static class VectorExtensions {

		public static float DistanceTo( this Vector3 a, Vector3 other ) => Vector3.Distance( a, other );

	}
}
