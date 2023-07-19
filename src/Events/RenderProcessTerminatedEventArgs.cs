using System;

namespace CefNet
{
	/// <summary>
	/// Provides data for the <see cref="IChromiumWebView.LoadError"/> event.
	/// </summary>
	public sealed class RenderProcessTerminatedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RenderProcessTerminatedEventArgs"/> class.
		/// </summary>
		/// <param name="status">Indicates how the process terminated</param>
		public RenderProcessTerminatedEventArgs(CefTerminationStatus status)
		{
			this.Status = status;
		}

		/// <summary>
		/// Indicates how the process terminated.
		/// </summary>
		public CefTerminationStatus Status { get; }
	}
}
