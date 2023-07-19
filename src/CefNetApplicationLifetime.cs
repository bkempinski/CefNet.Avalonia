﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace CefNet
{
    public class CefNetApplicationLifetime : IClassicDesktopStyleApplicationLifetime, IControlledApplicationLifetime, IApplicationLifetime, IDisposable
	{
		private int _exitCode;
		private CancellationTokenSource _cts;
		private bool _isShuttingDown;
		private HashSet<Window> _windows = new HashSet<Window>();
		private static CefNetApplicationLifetime _activeLifetime;


		/// <inheritdoc />
		public string[] Args { get; internal set; }

		/// <inheritdoc />
		public ShutdownMode ShutdownMode { get; set; }

		/// <inheritdoc />
		public Window MainWindow { get; set; }

		/// <inheritdoc />
		public IReadOnlyList<Window> Windows
		{
			get { return _windows.ToList(); }
		}

		/// <inheritdoc />
		public event EventHandler<ControlledApplicationLifetimeStartupEventArgs> Startup;

		/// <inheritdoc />
		public event EventHandler<ControlledApplicationLifetimeExitEventArgs> Exit;

		/// <inheritdoc />
		public event EventHandler<ShutdownRequestedEventArgs> ShutdownRequested;

		static CefNetApplicationLifetime()
		{
			Window.WindowOpenedEvent.AddClassHandler(typeof(Window), new EventHandler<RoutedEventArgs>(OnWindowOpened));
			Window.WindowClosedEvent.AddClassHandler(typeof(Window), new EventHandler<RoutedEventArgs>(WindowClosedEvent));
		}

		private static void WindowClosedEvent(object sender, RoutedEventArgs e)
		{
			var window = sender as Window;
			if (window is null)
				return;
			_activeLifetime?._windows.Remove(window);
			_activeLifetime?.HandleWindowClosed(window);
		}

		private static void OnWindowOpened(object sender, RoutedEventArgs e)
		{
			var window = sender as Window;
			if (window is null)
				return;
			_activeLifetime?._windows.Add(window);
		}

		public CefNetApplicationLifetime()
		{
			if (_activeLifetime is null)
				_activeLifetime = this;
			else
				throw new InvalidOperationException($"Can not have multiple active {this.GetType().Name} instances and the previously created one was not disposed.");
		}

		protected virtual void HandleWindowClosed(Window window)
		{
			if (window != null && !_isShuttingDown)
			{
				if (ShutdownMode == ShutdownMode.OnLastWindowClose && _windows.Count == 0)
				{
					Shutdown();
				}
				else if (ShutdownMode == ShutdownMode.OnMainWindowClose && window == MainWindow)
				{
					Shutdown();
				}
			}
		}

		public void OnShutdownRequested(object sender, ShutdownRequestedEventArgs e)
		{
			ShutdownRequested?.Invoke(this, e);

			if (e.Cancel)
				return;

			// When an OS shutdown request is received, try to close all non-owned windows. Windows can cancel
			// shutdown by setting e.Cancel = true in the Closing event. Owned windows will be shutdown by their
			// owners.
			foreach (var w in Windows)
				if (w.Owner is null)
					w.Close();
			if (Windows.Count > 0)
				e.Cancel = true;
		}

		/// <inheritdoc />
		public bool TryShutdown(int exitCode = 0)
		{
			var e = new ShutdownRequestedEventArgs();
			OnShutdownRequested(this, e);
			return !e.Cancel;
		}

		public void Shutdown(int exitCode = 0)
		{
			if (_isShuttingDown)
				throw new InvalidOperationException("Application is already shutting down.");

			_exitCode = exitCode;

			_isShuttingDown = true;
			try
			{
				foreach (Window window in Windows)
				{
					window.Close();
				}
				CefNetApplication.Instance.SignalForShutdown(ShutdownComplete);
			}
			catch
			{
				ShutdownComplete();
				throw;
			}
		}

		private void ShutdownComplete()
		{
			_cts?.Cancel();
			_cts = null;
			_isShuttingDown = false;
		}

		public int Start(string[] args)
		{
			try
			{
				this.Startup?.Invoke(this, new ControlledApplicationLifetimeStartupEventArgs(args));

				_cts = new CancellationTokenSource();
				MainWindow?.Show();

				Dispatcher.UIThread.MainLoop(_cts.Token);

				ControlledApplicationLifetimeExitEventArgs e = new ControlledApplicationLifetimeExitEventArgs(_exitCode);
				this.Exit?.Invoke(this, e);

				Environment.ExitCode = e.ApplicationExitCode;
				return e.ApplicationExitCode;
			}
			finally
			{
				CefNetApplication.Instance.Shutdown();
			}
		}

		public void Dispose()
		{
			if (_activeLifetime == this)
				_activeLifetime = null;
		}
		
	}

	public static class CefNetApplicationLifetimeExtensions
	{
		public static int StartWithCefNetApplicationLifetime(this AppBuilder builder, string[] args, ShutdownMode shutdownMode = ShutdownMode.OnLastWindowClose)
		{
			CefNetApplicationLifetime lifetime = new CefNetApplicationLifetime
			{
				Args = args,
				ShutdownMode = shutdownMode
			};
			builder.SetupWithLifetime(lifetime);
			return lifetime.Start(args);
		}
	}
}
