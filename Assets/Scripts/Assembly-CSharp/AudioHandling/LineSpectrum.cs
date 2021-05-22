#if UNITY_STANDALONE_WIN

using System.Collections.Generic;
using System.Linq;
using CSCore.DSP;

namespace SphereVisualizer.AudioHandling {
	internal class LineSpectrum : SpectrumBase {
		public int BarCount {
			get => SpectrumResolution;
			set => SpectrumResolution = value;
		}

		public LineSpectrum( FftSize fftSize ) {
			FftSize = fftSize;
		}

		public float[] GetSpectrumData( double maxValue ) {
			// Get spectrum data internal
			var fftBuffer = new float[( int )FftSize];

			UpdateFrequencyMapping();

			if ( !SpectrumProvider.GetFftData( fftBuffer, this ) ) {
				return null;
			}
			
			var spectrumPoints = CalculateSpectrumPoints( maxValue, fftBuffer ).ToList();

			// Convert to float[]
			var spectrumData = new List<float>();

			for ( var i = 0; i < spectrumPoints.Count; ++i ) {
				spectrumData.Add( ( float )spectrumPoints[i].Value );
			}

			return spectrumData.ToArray();

		}
	}
}
#endif
