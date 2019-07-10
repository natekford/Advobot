﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Advobot.NetCoreUI.Classes.ViewModels;
using Advobot.NetCoreUI.Classes.Views;
using AdvorangesUtils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging.Serilog;
using Avalonia.Threading;
using ReactiveUI;

namespace Advobot.NetCoreUI
{
	public sealed class NetCoreUILauncher
	{
		public static async Task Main(string[] args)
		{
			var services = await AdvobotLauncher.NoConfigurationStart(args).CAF();
			BuildAvaloniaApp().Start((app, _) => AppMain(app, services), args);
		}
		public static void AppMain(Application app, IServiceProvider services)
		{
			var cts = new CancellationTokenSource();

			new AdvobotNetCoreWindow
			{
				DataContext = new AdvobotNetCoreWindowViewModel(services),
			}.Show();
			
			app.Run(cts.Token);
		}
		public static AppBuilder BuildAvaloniaApp()
		{
			var app = AppBuilder.Configure<AdvobotNetCoreApp>().UsePlatformDetect().LogToDebug();
			app.AfterSetup(_ => RxApp.MainThreadScheduler = AvaloniaScheduler.Instance);
			return app;
		}
	}
}
