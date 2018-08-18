using System.Threading;
using System.Threading.Tasks;
using Advobot.Console;
using Advobot.NetCoreUI.Classes.ViewModels;
using AdvorangesUtils;
using Avalonia;
using Avalonia.Logging.Serilog;

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
			await launcher.Start().CAF();

			BuildAvaloniaApp().Start<AdvobotNetCoreWindow>(() => new AdvobotNetCoreWindowViewModel(launcher.GetServiceProvider()));
		}

		public static AppBuilder BuildAvaloniaApp()
		{
			return AppBuilder.Configure<AdvobotNetCoreApp>().UsePlatformDetect().UseReactiveUI().LogToDebug();
		}
	}
}
