using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SphereVisualizer {
	// This class is the core of the visualizer, it holds data that will be shared across all children.
	public class VisualizerManager : MonoBehaviour {
		public static VisualizerManager Singleton { get; private set; }

		public float Mass => mass;

		public float Scale => scale;

		public float Limit => limit;

		[SerializeField]
		[Tooltip( "The number of reactive boxes to show on screen. Must be divisible by 2." )]
		private byte bands = 16;

		[SerializeField]
		[Tooltip( "The random seed to use. Set to -1 to generate a random seed at start." )]
		private int seed;

		[SerializeField]
		[Tooltip( "Where the first orbit should be at." )]
		private uint startRadius = 8;

		[SerializeField]
		[Tooltip( "How much the orbit's max offset is." )]
		private float maxOrbitOffset = 0.25f;

		[SerializeField]
		[Tooltip( "How much the next orbit should be away from the last." )]
		private float orbitStepping = 1f;

		[SerializeField]
		[Tooltip( "The mass at the center." )]
		private float mass = 1000f;

		[SerializeField]
		[Tooltip( "The scale of reaction to the audio." )]
		[Range( 0.1f, 2f )]
		private float scale = 2f;

		[SerializeField]
		[Tooltip( "The size limit of the object." )]
		[Range( 2f, 100f )]
		private float limit = 5f;

		[SerializeField]
		[Tooltip( "The object to spawn that reacts to the bands it receives." )]
		private GameObject bandPrefab;

		[SerializeField]
		[GradientUsage( true )]
		[Tooltip( "Manages the color of the children." )]
		private Gradient mainGradient;

		public Color GetColor( float time ) => mainGradient.Evaluate( time );

		private void Awake() {
			if ( Singleton != null ) {
				Debug.LogError( $"A Visualizer Manager already exists, please make sure only 1 is in the scene.\n{Singleton.transform}" );

				Destroy( this );

				return;
			}

			Singleton = this;

			// Checks to see if seed is set to -1, and if so, to generate a "random" seed.
			if ( seed == -1 ) {
				var now = DateTime.Now;

				seed = ( now.Day | now.Month | now.Year ) & ( now.Hour | now.Minute | now.Second );
			}

			// Just because I think it looks cool, it will spawn in the orbiters every so often. 
			StartCoroutine( GenerateSphere() );
		}

		private IEnumerator GenerateSphere() {
			// Reverts the Random state back to default as to not mess with any other random system.
			var originalState = Random.state;

			// Meant to initialize the random seed into the Random class.
			Random.InitState( seed );

			// This is to make sure all bands are looped over.
			for ( var i = 0; i < bands; ++i ) {
				// This section right here calculates the orbit and position of the orbiter.
				var rad   = startRadius + ( orbitStepping + Random.Range( -maxOrbitOffset, maxOrbitOffset ) ) * i;
				var theta = 2 * Mathf.PI / bands * Random.Range( 0f, bands ) * i;
				var x     = rad * Mathf.Sin( theta );
				var y     = rad * Mathf.Cos( theta );
				var pos   = new Vector3( x, y, 0 );

				// Set's the default Quaternion.
				// I really don't care as orientation isn't an issue but it seems like I need to set one.
				var child = Instantiate( bandPrefab, pos, Quaternion.identity, transform );
				var reactor = child.GetComponent<Reactor>();

				reactor.Band = ( byte ) i;

				// Grabs the instance of the current state for revival once this Coroutine comes back.
				var myState        = Random.state;
				var points         = new List<Vector3>();
				// var lineRenderTask = new Task( () => CalculateOrbit( rad, points ) );

				// Set's the Random state back to the original state.
				Random.state = originalState;

				// lineRenderTask.Start();

				// Tell's the engine to yield this coroutine for 100 ms.

				yield return new WaitForSeconds( 0.1f );
				//yield return new WaitUntil( () => lineRenderTask.IsCompleted );

				// var lineRenderer = child.GetComponent<LineRenderer>();
				//
				// lineRenderer.alignment     = LineAlignment.View;
				// lineRenderer.endWidth      = 0.25f;
				// lineRenderer.startWidth    = 0.25f;
				// lineRenderer.useWorldSpace = true;
				// lineRenderer.loop          = true;
				// lineRenderer.startColor    = Color.white;
				// lineRenderer.endColor      = Color.white;
				// lineRenderer.positionCount = points.Count;
				//
				// lineRenderer.SetPositions( points.ToArray() );
				// lineRenderer.Simplify( 0.125f );

				// Resumes the current random state.
				Random.state = myState;
			}

			// Makes sure to revert back to the original random state.
			Random.state = originalState;

			static void CalculateOrbit( in float radius, ICollection<Vector3> positions ) {
				for ( var theta = 0f; theta < 2 * Mathf.PI; theta += 0.1f ) {
					var x   = radius * Mathf.Sin( theta );
					var y   = radius * Mathf.Cos( theta );
					var pos = new Vector3( x, y, 0 );

					positions.Add( pos );
				}
			}
		}
	}
}
