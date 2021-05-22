#if UNITY_STANDALONE_WIN

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using CSCore;
using CSCore.DSP;

namespace SphereVisualizer.AudioHandling {
	internal class SpectrumBase : INotifyPropertyChanged {
		private const int ScaleFactorLinear = 9;
		protected const int ScaleFactorSqr = 2;
		protected const double MinDbValue = -90;
		protected const double MaxDbValue = 0;
		protected const double DbScale = ( MaxDbValue - MinDbValue );

		private int fftSize;
		private bool isXLogScale;
		private int maxFftIndex;
		private int maximumFrequency = 20000;
		private int maximumFrequencyIndex;
		private int minimumFrequency = 20; //Default spectrum from 20Hz to 20kHz
		private int minimumFrequencyIndex;
		private int[] spectrumIndexMax;
		private int[] spectrumLogScaleIndexMax;

		private ISpectrumProvider spectrumProvider;

		protected int SpectrumResolution;
		private bool _useAverage;

		public int MaximumFrequency {
			get => maximumFrequency;
			set {
				if ( value <= MinimumFrequency ) {
					throw new ArgumentOutOfRangeException( "value",
						"Value must not be less or equal the MinimumFrequency." );
				}
				maximumFrequency = value;
				UpdateFrequencyMapping();

				RaisePropertyChanged( "MaximumFrequency" );
			}
		}

		public int MinimumFrequency {
			get => minimumFrequency;
			set {
				if ( value < 0 ) {
					throw new ArgumentOutOfRangeException( "value" );
				}

				minimumFrequency = value;
				UpdateFrequencyMapping();

				RaisePropertyChanged( "MinimumFrequency" );
			}
		}

		[Browsable( false )]
		public ISpectrumProvider SpectrumProvider {
			get => spectrumProvider;
			set {
				if ( value == null ) {
					throw new ArgumentNullException( "value" );
				}

				spectrumProvider = value;

				RaisePropertyChanged( "SpectrumProvider" );
			}
		}

		public bool IsXLogScale {
			get => isXLogScale;
			set {
				isXLogScale = value;
				UpdateFrequencyMapping();
				RaisePropertyChanged( "IsXLogScale" );
			}
		}

		public bool UseAverage {
			get => _useAverage;
			set {
				_useAverage = value;
				RaisePropertyChanged( "UseAverage" );
			}
		}

		[Browsable( false )]
		public FftSize FftSize {
			get => ( FftSize )fftSize;
			protected set {
				if ( ( int )Math.Log( ( int )value, 2 ) % 1 != 0 ) {
					throw new ArgumentOutOfRangeException( "value" );
				}

				fftSize = ( int )value;
				maxFftIndex = fftSize / 2 - 1;

				RaisePropertyChanged( "FFTSize" );
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void UpdateFrequencyMapping() {
			maximumFrequencyIndex = Math.Min( spectrumProvider.GetFftBandIndex( MaximumFrequency ) + 1, maxFftIndex );
			minimumFrequencyIndex = Math.Min( spectrumProvider.GetFftBandIndex( MinimumFrequency ), maxFftIndex );

			var actualResolution = SpectrumResolution;

			var indexCount = maximumFrequencyIndex - minimumFrequencyIndex;
			var linearIndexBucketSize = Math.Round( indexCount / ( double )actualResolution, 3 );

			spectrumIndexMax = spectrumIndexMax.CheckBuffer( actualResolution, true );
			spectrumLogScaleIndexMax = spectrumLogScaleIndexMax.CheckBuffer( actualResolution, true );

			var maxLog = Math.Log( actualResolution, actualResolution );
			for ( var i = 1; i < actualResolution; i++ ) {
				var logIndex =
					( int )( ( maxLog - Math.Log( ( actualResolution + 1 ) - i, ( actualResolution + 1 ) ) ) * indexCount ) +
					minimumFrequencyIndex;

				spectrumIndexMax[i - 1] = minimumFrequencyIndex + ( int )( i * linearIndexBucketSize );
				spectrumLogScaleIndexMax[i - 1] = logIndex;
			}

			if ( actualResolution > 0 ) {
				spectrumIndexMax[spectrumIndexMax.Length - 1] =
					spectrumLogScaleIndexMax[spectrumLogScaleIndexMax.Length - 1] = maximumFrequencyIndex;
			}
		}

		protected virtual SpectrumPointData[] CalculateSpectrumPoints( double maxValue, float[] fftBuffer ) {
			var dataPoints = new List<SpectrumPointData>();

			double value0 = 0, value = 0;
			double lastValue = 0;
			var actualMaxValue = maxValue;
			var spectrumPointIndex = 0;

			for ( var i = minimumFrequencyIndex; i <= maximumFrequencyIndex; i++ ) {
				//switch ( ScalingStrategy ) {
				//	case ScalingStrategy.Decibel:
				//		value0 = ( ( ( 20 * Math.Log10( fftBuffer[i] ) ) - MinDbValue ) / DbScale ) * actualMaxValue;
				//		break;
				//	case ScalingStrategy.Linear:
				//		value0 = ( fftBuffer[i] * ScaleFactorLinear ) * actualMaxValue;
				//		break;
				//	case ScalingStrategy.Sqrt:
				//		value0 = ( ( Math.Sqrt( fftBuffer[i] ) ) * ScaleFactorSqr ) * actualMaxValue;
				//		break;
				//}
				value0 = ( ( Math.Sqrt( fftBuffer[i] ) ) * ScaleFactorSqr ) * actualMaxValue;

				var recalc = true;

				value = Math.Max( 0, Math.Max( value0, value ) );

				while ( spectrumPointIndex <= spectrumIndexMax.Length - 1
					&& i == ( IsXLogScale ? spectrumLogScaleIndexMax[spectrumPointIndex] : spectrumIndexMax[spectrumPointIndex] ) ) {

					if ( !recalc ) {
						value = lastValue;
					}

					if ( value > maxValue ) {
						value = maxValue;
					}

					if ( _useAverage && spectrumPointIndex > 0 ) {
						value = ( lastValue + value ) / 2.0;
					}

					dataPoints.Add( new SpectrumPointData { SpectrumPointIndex = spectrumPointIndex, Value = value } );

					lastValue = value;
					value = 0.0;
					spectrumPointIndex++;
					recalc = false;
				}

				//value = 0;
			}

			return dataPoints.ToArray();
		}

		protected void RaisePropertyChanged( string propertyName ) {
			if ( PropertyChanged != null && !string.IsNullOrEmpty( propertyName ) ) {
				PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
			}
		}

		[DebuggerDisplay( "{Value}" )]
		protected struct SpectrumPointData {
			public int SpectrumPointIndex;
			public double Value;
		}
	}
}
#endif
