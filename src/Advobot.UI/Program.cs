using Advobot.UI.ViewModels;
using Advobot.UI.Views;

using AdvorangesUtils;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

using ReactiveUI;

namespace Advobot.UI;

public sealed class NetCoreUILauncher
{
	public static void AppMain(Application app, IServiceProvider services)
	{
		using var cts = new CancellationTokenSource();

		new AdvobotNetCoreWindow
		{
			DataContext = new AdvobotNetCoreWindowViewModel(services),
		}.Show();

		app.Run(cts.Token);
	}

	public static AppBuilder BuildAvaloniaApp()
	{
		var app = AppBuilder.Configure<AdvobotApp>().UsePlatformDetect().LogToDebug();
		app.AfterSetup(_ => RxApp.MainThreadScheduler = AvaloniaScheduler.Instance);
		return app;
	}

	public static async Task Main(string[] args)
	{
		var services = await AdvobotLauncher.NoConfigurationStart(args).CAF();
		BuildAvaloniaApp().Start((app, _) => AppMain(app, services), args);
	}
}