using System;
using SphereVisualizer.Extensions;
using UnityEngine;

namespace SphereVisualizer {
	public class Orbiter : MonoBehaviour {
		private const float G = 6.6743f;

		public float Mass { get; set; } = 5;

		private VisualizerManager center;

		private void Awake() {
			var myTransform = transform;

			center = myTransform.parent.GetComponent<VisualizerManager>();
		}

		private void Update() {
			// Simply calculates the needed force to continue orbiting.
			var position = transform.position;
			var dir      = center.transform.position - position;
			var forceMag = ( G * center.Mass * Mass ) / Mathf.Pow( dir.magnitude, 2 );
			var force    = dir.normalized * forceMag;

			force = Quaternion.Euler( 0, 0, 90 ) * force;

			var newPos = position + force * Time.deltaTime;
			transform.position = newPos;

			Debug.DrawLine( position, position + force * 2f, Color.red, Time.deltaTime );
		}
	}
}
