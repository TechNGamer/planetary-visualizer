using System;
using System.Threading.Tasks;

namespace SphereVisualizer.Extensions {
	public static class TaskExtensions {
		public static void FireAndForget( this Task task, Action<Exception> exceptionCallback = null ) {
			_ = Task.Run( async () => {
				try {
					task.Start();

					await task.ConfigureAwait( false );
				} catch ( Exception e ) {
					exceptionCallback?.Invoke( e );
				}
			} );
		}
	}
}
