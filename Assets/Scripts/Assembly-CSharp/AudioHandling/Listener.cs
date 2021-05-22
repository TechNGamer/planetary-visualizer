#if UNITY_STANDALONE_WIN
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CSCore;
using CSCore.CoreAudioAPI;
using CSCore.DSP;
using CSCore.SoundIn;
using CSCore.Streams;
using SphereVisualizer.Extensions;
using UnityEngine;

namespace SphereVisualizer.AudioHandling {
	public class Listener {
		public static event Action<bool, float[]> ReceiveSpectrumDataAction;

		private static LinkedList<Listener> singletons = new LinkedList<Listener>();

		static Listener() {
			var deviceEnum = new MMDeviceEnumerator();

			using ( var deviceCollection = deviceEnum.EnumAudioEndpoints( DataFlow.Render, DeviceState.Active ) ) {
				foreach ( var mmDevice in deviceCollection ) {
					var listener = new Listener( mmDevice );

					singletons.AddLast( listener );
				}
			}

			Application.quitting += Quitting;

			new Task( CheckForDefaultChange ).FireAndForget();
			new Task( CheckForDeviceChange ).FireAndForget();
		}

		private static void Quitting() {
			var node = singletons.First;

			while ( node != null ) {
				try {
					node.Value.StopListening();
				} catch ( ObjectDisposedException ) {
					Debug.Log( $"{node.Value.loopback.Device.FriendlyName}: Object already disposed." );
				}

				singletons.Remove( node );

				node = node.Next;
			}
		}

		[SuppressMessage( "ReSharper", "FunctionNeverReturns", Justification = "Method is meant to be looping forever." )]
		private static void CheckForDeviceChange() {
			while ( true ) {
				Task.Delay( 128 ).Wait();

				using var deviceEnum       = new MMDeviceEnumerator();
				using var deviceCollection = deviceEnum.EnumAudioEndpoints( DataFlow.Render, DeviceState.Active );
				lock ( singletons ) {
					if ( deviceCollection.Count == singletons.Count ) {
						continue;
					}

					var newListeners = new LinkedList<Listener>();

					foreach ( var mmDevice in deviceCollection ) {
						var listenerPresent = false;

						foreach ( var listener in singletons.Where( listener => listener.OutDevice.DeviceID == mmDevice.DeviceID ) ) {
							listenerPresent = true;

							newListeners.AddLast( listener );

							break;
						}

						if ( listenerPresent ) {
							Debug.Log( $"Listener for device `{mmDevice.DeviceID}/{mmDevice.FriendlyName}` doesn't need to die." );

							continue;
						}

						Debug.Log( $"Creating new listener for `{mmDevice.DeviceID}/{mmDevice.FriendlyName}` since one does not exist." );

						newListeners.AddLast( new Listener( mmDevice ) );
					}

					singletons = newListeners;
				}
			}
		}

		[SuppressMessage( "ReSharper", "FunctionNeverReturns", Justification = "Method is meant to be looping forever." )]
		private static void CheckForDefaultChange() {
			while ( true ) {
				Task.Delay( 256 ).Wait();

				using var deviceEnum      = new MMDeviceEnumerator();
				var       defaultDevice   = deviceEnum.GetDefaultAudioEndpoint( DataFlow.Render, Role.Multimedia );
				var       alreadyFoundOut = false;

				lock ( singletons ) {
					foreach ( var listener in singletons ) {
						if ( alreadyFoundOut || !listener.OutDevice.DeviceID.Equals( defaultDevice.DeviceID, StringComparison.OrdinalIgnoreCase ) ) {
							listener.IsDefaultOut = false;

							continue;
						}

						listener.IsDefaultOut = true;
						alreadyFoundOut       = true;
					}
				}
			}
		}

		private bool IsDefaultOut { get; set; }

		private MMDevice OutDevice => loopback.Device;

		private float[] buffer;

		private readonly WasapiLoopbackCapture loopback = new WasapiLoopbackCapture();
		private SoundInSource soundIn;
		private SingleBlockNotificationStream blockNotifyStream;
		private BasicSpectrumProvider spectrumProvider;
		private LineSpectrum spectrum;

		private IWaveSource realtime;

		private Listener( MMDevice device ) {
			loopback.Device = device;

			CreateLoopback();

			Application.quitting += StopListening;
		}

		~Listener() {
			Application.quitting -= StopListening;

			StopListening();
		}

		private void StopListening() {
			blockNotifyStream.SingleBlockRead -= SingleBlockRead;

			soundIn.Dispose();
			realtime.Dispose();

			loopback?.Stop();
			loopback?.Dispose();
		}

		[SuppressMessage( "Design", "CA1031:Do not catch general exception types", Justification = "It seems to be weird, so it stays." )]
		private void CreateLoopback() {
			try {
				loopback.Initialize();
			} catch ( Exception e ) {
				Debug.LogException( e );

				return;
			}

			soundIn = new SoundInSource( loopback );
			spectrumProvider = new BasicSpectrumProvider( soundIn.WaveFormat.Channels, soundIn.WaveFormat.SampleRate, FftSize.Fft4096 );
			spectrum = new LineSpectrum( FftSize.Fft4096 ) {
				SpectrumProvider = spectrumProvider,
				BarCount = 512,
				UseAverage = true,
				IsXLogScale = true,
			};

			loopback.Start();

			blockNotifyStream = new SingleBlockNotificationStream( soundIn.ToSampleSource() );
			realtime = blockNotifyStream.ToWaveSource();

			buffer = new float[realtime.WaveFormat.BytesPerSecond / sizeof( float ) / 2];

			soundIn.DataAvailable += AudioDataAvailable;

			blockNotifyStream.SingleBlockRead += SingleBlockRead;
		}

		private void SingleBlockRead( object sender, SingleBlockReadEventArgs e ) => spectrumProvider.Add( e.Left, e.Right );

		private void AudioDataAvailable( object sender, DataAvailableEventArgs e ) {
			var byteBuffer = new byte[buffer.Length * sizeof( float )];

			while ( realtime.Read( byteBuffer, 0, byteBuffer.Length ) > 0 ) {
				var spectrumData = spectrum.GetSpectrumData( 10 );

				if ( spectrumData != null ) {
					ReceiveSpectrumDataAction?.Invoke( IsDefaultOut, spectrumData );
				}
			}
		}
	}
}
#endif
