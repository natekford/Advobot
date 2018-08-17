using System;
using System.Threading;
using System.Threading.Tasks;
using Advobot.Console;
using Advobot.NetCoreUI.Classes.ViewModels;
using AdvorangesUtils;
using Avalonia;
using Avalonia.Logging.Serilog;
using Avalonia.Threading;
using ReactiveUI;

namespace Advobot.NetCoreUI
{
	public sealed class NetCoreUILauncher
	{
		private static async Task Main(string[] args)
		{
			Thread.CurrentThread.TrySetApartmentState(ApartmentState.STA);

			var launcher = new AdvobotNetCoreLauncher(args);
			launcher.SetPath();
			await launcher.SetBotKey().CAF();
			//Run the actual Discord bot first, then use this to let the UI also run at the same time but also throw unhandled exceptions
			_ = Task.Run(async () => await launcher.Start().CAF()).ContinueWith(t =>
			{
				ThreadPool.QueueUserWorkItem(w =>
				{
					if (t.Exception != null)
					{
						foreach (var e in t.Exception.InnerExceptions)
						{
							throw new InvalidOperationException("An unhandled exception has occurred in a background thread.", e);
						}
					}
				});
			}, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.PreferFairness);

			BuildAvaloniaApp().Start<AdvobotNetCoreWindow>(() => new AdvobotNetCoreWindowViewModel(launcher.GetServiceProvider()));
		}

		public static AppBuilder BuildAvaloniaApp()
		{
			var app = AppBuilder.Configure<AdvobotNetCoreApp>().UsePlatformDetect().LogToDebug();
			app.AfterSetup(_ =>
			{
				RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
			});
			return app;
		}
	}
}
