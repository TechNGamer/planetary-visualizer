#if UNITY_STANDALONE_WIN

using System.Collections.Generic;
using CSCore.DSP;

namespace SphereVisualizer.AudioHandling {
	public class BasicSpectrumProvider : FftProvider, ISpectrumProvider {

		private readonly int sampleRate;

		private readonly List<object> contexts = new List<object>();

		public BasicSpectrumProvider( int channels, int sampleRate, FftSize fftSize ) : base( channels, fftSize ) {
			this.sampleRate = sampleRate;
		}

		public int GetFftBandIndex( float freq ) {
			var fftSize = ( int )FftSize;
			var f = sampleRate / 2.0;

			return ( int )( freq / f * ( fftSize / 2f ) );
		}

		public bool GetFftData( float[] fftBuffer, object context ) {
			//if ( contexts.Contains( context ) ) {
			//	return false;
			//}

			//contexts.Add( context );
			GetFftData( fftBuffer );

			return true;
		}

		public new void Add( float[] samples, int count ) {
			base.Add( samples, count );

			if ( count > 0 ) {
				contexts.Clear();
			}
		}

		public new void Add( float left, float right ) {
			base.Add( left, right );
			contexts.Clear();
		}
	}

	public interface ISpectrumProvider {

		bool GetFftData( float[] fftBuffer, object context );
		int GetFftBandIndex( float freq );

	}
}
#endif
