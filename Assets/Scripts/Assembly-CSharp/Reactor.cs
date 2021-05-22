using System;
using System.Collections;
using SphereVisualizer.AudioHandling;
using UnityEngine;

namespace SphereVisualizer {
	public class Reactor : MonoBehaviour {
		const                   int NUM_OF_SEGMENTS = 120;
		private static readonly int SHADER_COLOR    = Shader.PropertyToID( "_MainColor" );

		public byte Band { get; set; }

		private int               bandOffset;
		private VisualizerManager manager;

		private Processor    processor;
		private Material     sphereMat;
		private Material     lineMat;
		private LineRenderer lineRenderer;

		private void Awake() {
			manager      = VisualizerManager.Singleton;
			processor    = transform.parent.GetComponent<Processor>();
			sphereMat    = GetComponent<MeshRenderer>().material;
			lineRenderer = GetComponent<LineRenderer>();
			lineMat      = lineRenderer.material;
		}

		private void Start() {
			bandOffset = Band + 1;

			lineRenderer.positionCount = NUM_OF_SEGMENTS * bandOffset;

			for ( var i = 0; i < NUM_OF_SEGMENTS * bandOffset; ++i ) {
				lineRenderer.SetPosition( i, transform.position );
			}

			StartCoroutine( UpdateWhenFarEnough() );
		}

		private void Update() {
			UpdateColor();
		}

		private IEnumerator UpdateWhenFarEnough() {
			var lastPos   = transform.position;
			var radUpdate = Mathf.Pow( 0.25f, bandOffset * ( 5f / bandOffset ) );

			while ( true ) {
				var pos       = transform.position;
				var angleDiff = Vector3.Angle( lastPos, pos ) * Mathf.Deg2Rad;

				if ( angleDiff >= radUpdate ) {
					UpdateLineRenderer();

					lastPos = pos;
				}

				yield return new WaitForEndOfFrame();
			}
		}

		private void UpdateLineRenderer() {
			for ( var i = NUM_OF_SEGMENTS * bandOffset - 1; i > 0; --i ) {
				lineRenderer.SetPosition( i, lineRenderer.GetPosition( i - 1 ) );
			}

			lineRenderer.SetPosition( 0, transform.position );
		}

		private void UpdateColor() {
			var newSize = Mathf.Clamp( processor[Band] * manager.Scale, 1f, manager.Limit );
			var time    = newSize / manager.Limit;

			transform.localScale = new Vector3( newSize, newSize, newSize );

			sphereMat.SetColor( SHADER_COLOR, manager.GetColor( time ) );
			lineMat.SetColor( SHADER_COLOR, manager.GetColor( time ) );
		}
	}
}
