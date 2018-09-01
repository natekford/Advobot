using System.Threading;
using System.Threading.Tasks;
using Advobot.NetCoreUI.Classes.ViewModels;
using Advobot.NetCoreUI.Classes.Views;
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

			var launcher = new AdvobotConsoleLauncher(args);
			launcher.SetPath();
			await launcher.SetBotKey().CAF();
			await launcher.Start().CAF();

			BuildAvaloniaApp().Start<AdvobotNetCoreWindow>(() => new AdvobotNetCoreWindowViewModel(launcher.GetServiceProvider()));
		}

		public static AppBuilder BuildAvaloniaApp()
		{
			var app = AppBuilder.Configure<AdvobotNetCoreApp>().UsePlatformDetect().LogToDebug();
			app.AfterSetup(_ =>
			{
				RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
				/* Not sure how to get this to work, uncaught exceptions in Avalonia either don't trigger it or don't work correctly
				AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
				{
					MessageBox.Show($"UNHANDLED EXCEPTION:\n\n{e.ExceptionObject}", "UNHANDLED EXCEPTION", null);
				};*/
			});
			return app;
		}
	}
}
