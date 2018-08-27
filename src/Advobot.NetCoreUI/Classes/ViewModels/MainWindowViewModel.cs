using Advobot.Interfaces;
using Advobot.NetCoreUI.Classes.Colors;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Newtonsoft.Json;
using Advobot.NetCoreUI.Utils;

namespace Advobot.NetCoreUI.Classes.ViewModels
{
	public class AdvobotNetCoreWindowViewModel : ReactiveObject
	{
		public string MainMenuText =>
			"Latency: Time it takes for a command to reach the bot.\n\n" + 
			"Memory: Amount of RAM the program is using.\n\n" +
			"Threads: Where all the actions in the bot happen.\n\n" +
			$"API wrapper version: {Constants.API_VERSION}\n" +
			$"Bot version: {Constants.BOT_VERSION}\n\n" +
			$"Github repository for Advobot: {Constants.REPO}" +
			$"Join the Discord server for additional help: {Constants.DISCORD_INV}";

		public string Output
		{
			get => _Output;
			set => this.RaiseAndSetIfChanged(ref _Output, value);
		}
		private string _Output = "";

		public string Input
		{
			get => _Input;
			set
			{
				this.RaiseAndSetIfChanged(ref _Input, value);
				CanInput = value.Length > 0;
			}
		}
		private string _Input = "";

		private bool CanInput
		{
			get => _CanInput;
			set => this.RaiseAndSetIfChanged(ref _CanInput, value);
		}
		private bool _CanInput;

		public string PauseButtonContent
		{
			get => _PauseButtonContent;
			set => this.RaiseAndSetIfChanged(ref _PauseButtonContent, value);
		}
		private string _PauseButtonContent;

		public BotSettingsViewModel BotSettingsViewModel
		{
			get => _BotSettingsViewModel;
			private set => this.RaiseAndSetIfChanged(ref _BotSettingsViewModel, value);
		}
		private BotSettingsViewModel _BotSettingsViewModel;

		public ColorsViewModel ColorsViewModel
		{
			get => _ColorsViewModel;
			private set => this.RaiseAndSetIfChanged(ref _ColorsViewModel, value);
		}
		private ColorsViewModel _ColorsViewModel;

		public int OutputColumnSpan
		{
			get => _OutputColumnSpan;
			private set => this.RaiseAndSetIfChanged(ref _OutputColumnSpan, value);
		}
		private int _OutputColumnSpan = 2;

		public bool OpenMainMenu => GetMenuStatus();
		public bool OpenInfoMenu => GetMenuStatus();
		public bool OpenColorsMenu => GetMenuStatus();
		public bool OpenSettingsMenu => GetMenuStatus();
		private ConcurrentDictionary<string, bool> OpenMenus = new ConcurrentDictionary<string, bool>();

		public IObservable<string> Uptime { get; }
		public IObservable<string> Latency { get; }
		public IObservable<string> Memory { get; }
		public IObservable<string> ThreadCount { get; }

		public ICommand OutputCommand { get; }
		public ICommand InputCommand { get; }
		public ICommand OpenMenuCommand { get; }
		public ICommand DisconnectCommand { get; }
		public ICommand RestartCommand { get; }
		public ICommand PauseCommand { get; }
		public ICommand OpenFileSearchWindowCommand { get; }
		public ICommand SaveColorsCommand { get; }
		public ICommand SaveBotSettingsCommand { get; }
		public ICommand ClearOutputCommand { get; }
		public ICommand SaveOutputCommand { get; }
		public ICommand OpenOutputSearchWindowCommand { get; }

		public DiscordShardedClient Client
		{
			get => _Client;
			set => this.RaiseAndSetIfChanged(ref _Client, value);
		}
		private DiscordShardedClient _Client = null;

		public IBotSettings BotSettings
		{
			get => _BotSettings;
			set => this.RaiseAndSetIfChanged(ref _BotSettings, value);
		}
		private IBotSettings _BotSettings = null;

		public ILogService LogService
		{
			get => _LogService;
			set => this.RaiseAndSetIfChanged(ref _LogService, value);
		}
		private ILogService _LogService = null;

		public NetCoreColorSettings Colors
		{
			get => _Colors;
			set => this.RaiseAndSetIfChanged(ref _Colors, value);
		}
		private NetCoreColorSettings _Colors = null;

		public AdvobotNetCoreWindowViewModel(IServiceProvider provider)
		{
			Client = provider.GetRequiredService<DiscordShardedClient>();
			BotSettings = provider.GetRequiredService<IBotSettings>();
			LogService = provider.GetRequiredService<ILogService>();
			Colors = NetCoreColorSettings.Load<NetCoreColorSettings>(BotSettings);

			BotSettingsViewModel = new BotSettingsViewModel(BotSettings);
			ColorsViewModel = new ColorsViewModel(Colors);

			OutputCommand = ReactiveCommand.Create<string>(x => Output += x);
			Console.SetOut(new TextBoxStreamWriter(OutputCommand));
			InputCommand = ReactiveCommand.Create(() =>
			{
				ConsoleUtils.WriteLine(Input, name: "UIInput");
				Input = "";
			}, this.WhenAnyValue(x => x.CanInput));
			OpenMenuCommand = ReactiveCommand.Create<string>(x =>
			{
				foreach (var key in new List<string>(OpenMenus.Keys))
				{
					//If not the targeted menu, set to false
					//If the targeted menu, toggle the visibility
					var currentValue = OpenMenus[key];
					var newValue = key == x && !currentValue;
					if (currentValue != newValue)
					{
						OpenMenus[key] = newValue;
						this.RaisePropertyChanged(key);
					}
				}
				OutputColumnSpan = OpenMenus.Any(kvp => kvp.Value) ? 1 : 2;
			});
			DisconnectCommand = ReactiveCommand.CreateFromTask(async () => await ClientUtils.DisconnectBotAsync(Client).CAF());
			RestartCommand = ReactiveCommand.CreateFromTask(async () => await ClientUtils.RestartBotAsync(Client, BotSettings).CAF());
			PauseCommand = ReactiveCommand.Create(() =>
			{
				PauseButtonContent = BotSettings.Pause ? "Pause" : "Unpause";
				ConsoleUtils.WriteLine($"The bot is now {(BotSettings.Pause ? "unpaused" : "paused")}.", name: "Pause");
				BotSettings.Pause = !BotSettings.Pause;
			});
			OpenFileSearchWindowCommand = ReactiveCommand.Create(() =>
			{
				throw new NotImplementedException();
			});
			SaveColorsCommand = ReactiveCommand.Create(() =>
			{
				ConsoleUtils.WriteLine("Successfully saved the color settings.", name: "Saving");
				Colors.SaveSettings(BotSettings);
			}, this.WhenAnyValue(x => x.OpenColorsMenu));
			SaveBotSettingsCommand = ReactiveCommand.Create(() =>
			{
				ConsoleUtils.WriteLine("Successfully saved the bot settings.", name: "Saving");
				BotSettings.SaveSettings(BotSettings);
			}, this.WhenAnyValue(x => x.OpenSettingsMenu));
			ClearOutputCommand = ReactiveCommand.Create(() => Output = "");
			SaveOutputCommand = ReactiveCommand.Create(() => NetCoreUIUtils.Save(BotSettings, "Output", Output));
			OpenOutputSearchWindowCommand = ReactiveCommand.Create(() =>
			{
				throw new NotImplementedException();
			});

			var timer = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1));
			var space = 23; //??????????????????????????????????????????????????????
			//Not sure how this happens, but unless the strings are past a specific length they fail to update in the UI.
			//The padding also has to be done on the right, not the left.
			//For the regular font size and only 4 textboxes inside the grid, this requires 29 characters.
			//For .02 dynamic font size and only 4 textboxes inside the grid, this requires 23 characters.
			//The more textboxes inside the grid, the less characters required because the less width each textbox gets.
			Uptime = timer.Select(x => $"Uptime: {ProcessInfoUtils.GetUptime():dd\\.hh\\:mm\\:ss}".PadRight(space));
			Latency = timer.Select(x => $"Latency: {(Client?.CurrentUser == null ? -1 : Client.Latency)}ms".PadRight(space));
			Memory = timer.Select(x => $"Memory: {ProcessInfoUtils.GetMemoryMB():0.00}MB".PadRight(space));
			ThreadCount = timer.Select(x => $"Threads: {ProcessInfoUtils.GetThreadCount()}".PadRight(space));
		}

		private bool GetMenuStatus([CallerMemberName] string menu = "")
		{
			return OpenMenus.GetOrAdd(menu, false);
		}
	}
}