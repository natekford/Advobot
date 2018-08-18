using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System.Linq;

namespace Advobot.NetCoreUI.Classes.ViewModels
{
	public class AdvobotNetCoreWindowViewModel : ReactiveObject
	{
		private string _Output = "";
		public string Output
		{
			get => _Output;
			set => this.RaiseAndSetIfChanged(ref _Output, value);
		}

		private string _Input = "";
		public string Input
		{
			get => _Input;
			set => this.RaiseAndSetIfChanged(ref _Input, value);
		}

		private string _PauseButtonContent;
		public string PauseButtonContent
		{
			get => _PauseButtonContent;
			set => this.RaiseAndSetIfChanged(ref _PauseButtonContent, value);
		}

		private bool _OpenMainMenu = false;
		public bool OpenMainMenu
		{
			get => _OpenMainMenu;
			set => this.RaiseAndSetIfChanged(ref _OpenMainMenu, value);
		}

		private bool _OpenInfoMenu = false;
		public bool OpenInfoMenu
		{
			get => _OpenInfoMenu;
			set => this.RaiseAndSetIfChanged(ref _OpenInfoMenu, value);
		}

		private bool _OpenColorsMenu = false;
		public bool OpenColorsMenu
		{
			get => _OpenColorsMenu;
			set => this.RaiseAndSetIfChanged(ref _OpenColorsMenu, value);
		}

		private bool _OpenSettingsMenu = false;
		public bool OpenSettingsMenu
		{
			get => _OpenSettingsMenu;
			set => this.RaiseAndSetIfChanged(ref _OpenSettingsMenu, value);
		}

		private DiscordShardedClient _Client = null;
		public DiscordShardedClient Client
		{
			get => _Client;
			set => this.RaiseAndSetIfChanged(ref _Client, value);
		}

		private IBotSettings _BotSettings = null;
		public IBotSettings BotSettings
		{
			get => _BotSettings;
			set => this.RaiseAndSetIfChanged(ref _BotSettings, value);
		}

		private ILogService _LogService = null;
		public ILogService LogService
		{
			get => _LogService;
			set => this.RaiseAndSetIfChanged(ref _LogService, value);
		}

		private NetCoreColorSettings _Colors = null;
		public NetCoreColorSettings Colors
		{
			get => _Colors;
			set => this.RaiseAndSetIfChanged(ref _Colors, value);
		}

		public IEnumerable<ColorTheme> Themes { get; } = Enum.GetValues(typeof(ColorTheme)).Cast<ColorTheme>();

		/*
		private string _Uptime = null;
		public string Uptime
		{
			get => _Uptime;
			set => this.RaiseAndSetIfChanged(ref _Uptime, value);
		}*/

		public IObservable<string> Uptime { get; }
		public IObservable<string> Latency { get; }
		public IObservable<string> Memory { get; }
		public IObservable<string> ThreadCount { get; }

		public ReactiveCommand OutputCommand { get; }
		public ReactiveCommand InputCommand { get; }
		public ReactiveCommand OpenMenuCommand { get; }
		public ReactiveCommand DisconnectCommand { get; }
		public ReactiveCommand RestartCommand { get; }
		public ReactiveCommand PauseCommand { get; }
		public ReactiveCommand SaveColorsCommand { get; }
		public ReactiveCommand SaveBotSettingsCommand { get; }

		public System.Timers.Timer Timer { get; }

		public AdvobotNetCoreWindowViewModel(IServiceProvider provider)
		{
			Client = provider.GetRequiredService<DiscordShardedClient>();
			BotSettings = provider.GetRequiredService<IBotSettings>();
			LogService = provider.GetRequiredService<ILogService>();
			Colors = NetCoreColorSettings.Load<NetCoreColorSettings>(BotSettings);

			OutputCommand = ReactiveCommand.Create<string>(x =>
			{
				Output += x;
			});
			Console.SetOut(new TextBoxStreamWriter(OutputCommand));
			InputCommand = ReactiveCommand.Create(() =>
			{
				ConsoleUtils.WriteLine(Input, name: "UIInput");
				Input = "";
			});
			OpenMenuCommand = ReactiveCommand.Create<string>(x =>
			{
				switch (x)
				{
					case nameof(OpenMainMenu):
						OpenInfoMenu = OpenColorsMenu = OpenSettingsMenu = false;
						OpenMainMenu = !OpenMainMenu && true;
						break;
					case nameof(OpenInfoMenu):
						OpenMainMenu = OpenColorsMenu = OpenSettingsMenu = false;
						OpenInfoMenu = !OpenInfoMenu && true;
						break;
					case nameof(OpenColorsMenu):
						OpenMainMenu = OpenInfoMenu = OpenSettingsMenu = false;
						OpenColorsMenu = !OpenColorsMenu && true;
						break;
					case nameof(OpenSettingsMenu):
						OpenMainMenu = OpenInfoMenu = OpenColorsMenu = false;
						OpenSettingsMenu = !OpenSettingsMenu && true;
						break;
				}
			});
			DisconnectCommand = ReactiveCommand.CreateFromTask(async () => await ClientUtils.DisconnectBotAsync(Client).CAF());
			RestartCommand = ReactiveCommand.CreateFromTask(async () => await ClientUtils.RestartBotAsync(Client, BotSettings).CAF());
			PauseCommand = ReactiveCommand.Create(() =>
			{
				PauseButtonContent = BotSettings.Pause ? "Pause" : "Unpause";
				ConsoleUtils.WriteLine($"The bot is now {(BotSettings.Pause ? "unpaused" : "paused")}.", name: "Pause");
				BotSettings.Pause = !BotSettings.Pause;
			});
			SaveColorsCommand = ReactiveCommand.Create(() =>
			{
				ConsoleUtils.WriteLine("Successfully saved the color settings.");
				Colors.SaveSettings(BotSettings);
			});
			SaveBotSettingsCommand = ReactiveCommand.Create(() =>
			{
				ConsoleUtils.WriteLine("Successfully saved the bot settings.");
				BotSettings.SaveSettings(BotSettings);
			});

			/*
			Timer = new System.Timers.Timer
			{
				Interval = 1000,
				Enabled = true,
			};
			Timer.Elapsed += (sender, e) =>
			{
				Dispatcher.UIThread.InvokeAsync(() => Uptime = $"Uptime: {FormattingUtils.GetUptime()}");
			};*/

			/*
			Timer = new DispatcherTimer
			{
				Interval = TimeSpan.FromSeconds(1),
				IsEnabled = true,
			};
			Timer.Tick += (sender, e) =>
			{
				Uptime = $"Uptime: {FormattingUtils.GetUptime()}";
			};*/

			var timer = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1));
			Uptime = timer.Select(x => $"Uptime: {FormattingUtils.GetUptime()}");
			Latency = timer.Select(x => $"Latency: {(Client?.CurrentUser == null ? -1 : Client.Latency)}ms");
			Memory = timer.Select(x => $"Memory: {IOUtils.GetMemory().ToString("0.00")}MB");
			ThreadCount = timer.Select(x => $"Threads: {Utilities.Utils.GetThreadCount()}");
		}
	}
}