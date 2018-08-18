using System.Threading;
using System.Threading.Tasks;
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
			});
			return app;
		}
	}
}
