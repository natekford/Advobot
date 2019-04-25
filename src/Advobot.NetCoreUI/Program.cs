using System;
using System.Threading.Tasks;
using Advobot.NetCoreUI.Classes.ViewModels;
using Advobot.NetCoreUI.Classes.Views;
using Advobot.Utilities;
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
			var launcher = new AdvobotConsoleLauncher(args);
			await launcher.GetPathAndKeyAsync().CAF();
			var services = launcher.GetDefaultServices(DiscordUtils.GetCommandAssemblies());
			var provider = launcher.CreateProvider(services);
			await launcher.StartAsync(provider).CAF();
			BuildAvaloniaApp().Start<AdvobotNetCoreWindow>(() => new AdvobotNetCoreWindowViewModel(provider));
		}
		public static AppBuilder BuildAvaloniaApp()
		{
			var app = AppBuilder.Configure<AdvobotNetCoreApp>().UsePlatformDetect().LogToDebug();
			app.AfterSetup(_ => RxApp.MainThreadScheduler = AvaloniaScheduler.Instance);
			return app;
		}
	}
}
