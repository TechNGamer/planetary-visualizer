using UnityEngine;

namespace SphereVisualizer {
	[RequireComponent( typeof( MeshFilter ), typeof( MeshRenderer ) )]
	public class Manipulator : MonoBehaviour {
		/// <summary>
		/// The points in object-space where the vertices are at.
		/// </summary>
		private static readonly Vector3[] VERTS = {
			new Vector3( 0f, 0f, 0f ), // Bottom Front Right.
			new Vector3( 1f, 0f, 0f ), // Bottom Back Right.
			new Vector3( 0f, 1f, 0f ), // Top Front Right.
			new Vector3( 1f, 1f, 0f ), // Top Back Right.
			new Vector3( 0f, 1f, 1f ), // Top Front Left.
			new Vector3( 1f, 1f, 1f ), // Top Back Left.
			new Vector3( 0f, 0f, 1f ), // Bottom Front Left.
			new Vector3( 1f, 0f, 1f ), // Bottom Back Left.
		};

		/// <summary>
		/// The triangles of the cube that is generated at runtime.
		/// </summary>
		/// <remarks>
		/// This cube is generated at runtime because the cube that Unity provides has a lot of vertices to it.
		/// This is meant to cut that number down, plus make it easier for the code to manipulate the vertices.
		/// </remarks>
		private static readonly int[] TRIS = {
			// Face 0
			1, 0, 2,
			2, 3, 1,
			// Face 1
			2, 4, 5,
			5, 3, 2,
			// Face 2
			4, 6, 5,
			5, 6, 7,
			// Face 3
			0, 1, 6,
			1, 7, 6,
			// Face 4
			0, 6, 4,
			4, 2, 0,
			// Face 5
			1, 3, 7,
			3, 5, 7,
		};

		// A simple shortcut method so I don't have to retype these three methods every time I need to refresh the mesh.
		private static void RecalculateMesh( Mesh mesh ) {
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.RecalculateTangents();
		}

		internal byte BandNumber { get; set; } = byte.MaxValue;

		[SerializeField]
		private MeshFilter meshFiler;
		

		private void Awake() {
			// Creates a new mesh so as to not interfere with the other objects.
			var mesh = new Mesh();
			// Creates a new Vector3 Array as to not interfere with the reference to the main one.
			var verts = new Vector3[VERTS.Length];

			for ( var i = 0; i < verts.Length; ++i ) {
				verts[i] = new Vector3( VERTS[i].x, VERTS[i].y, VERTS[i].z );
			}

			mesh.vertices = verts;
			// It is fine if the triangles reference the const as they will not be changes at all.
			mesh.triangles = TRIS;

			RecalculateMesh( mesh );

			meshFiler.sharedMesh = mesh;
		}
	}
}
