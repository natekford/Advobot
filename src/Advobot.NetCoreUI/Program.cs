using System;
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
		[STAThread]
		private static async Task Main(string[] args)
		{
			var services = await AdvobotLauncher.NoConfigurationStart(args).CAF();
			BuildAvaloniaApp().Start<AdvobotNetCoreWindow>(() => new AdvobotNetCoreWindowViewModel(services));
		}
		public static AppBuilder BuildAvaloniaApp()
		{
			var app = AppBuilder.Configure<AdvobotNetCoreApp>().UsePlatformDetect().LogToDebug();
			app.AfterSetup(_ => RxApp.MainThreadScheduler = AvaloniaScheduler.Instance);
			return app;
		}
	}
}
