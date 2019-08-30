using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

using Advobot.Services.BotSettings;
using Advobot.Services.Logging;
using Advobot.UI.Colors;
using Advobot.UI.Controls;
using Advobot.UI.Utils;
using Advobot.UI.Views;
using Advobot.Utilities;

using AdvorangesUtils;

using Avalonia.Controls;
using Avalonia.Media;

using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;

namespace Advobot.UI.ViewModels
{
	public sealed class AdvobotNetCoreWindowViewModel : ReactiveObject
	{
		private static readonly string _Caption = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;

		private readonly IBotSettings _BotSettings;

		private readonly BaseSocketClient _Client;

		private readonly IColorSettings<ISolidColorBrush> _Colors;

		private readonly ILogService _LogService;

		private readonly ConcurrentDictionary<string, bool> _MenuStatuses = new ConcurrentDictionary<string, bool>();

		private string _Input = "";

		private string _Output = "";

		private int _OutputColumnSpan = 2;

		private string _PauseButtonContent = "";

		public AdvobotNetCoreWindowViewModel(IServiceProvider provider)
		{
			_Client = provider.GetRequiredService<BaseSocketClient>();
			_LogService = provider.GetRequiredService<ILogService>();
			_BotSettings = provider.GetRequiredService<IBotSettings>();
			_Colors = NetCoreColorSettings.CreateOrLoad(_BotSettings);

			LogServiceViewModel = new LogServiceViewModel(_LogService);
			BotSettingsViewModel = new BotSettingsViewModel(_BotSettings);
			ColorsViewModel = new ColorsViewModel(_Colors);

			PrintOutputCommand = ReactiveCommand.Create<string>(PrintOutput);
			Console.SetOut(new TextBoxStreamWriter(PrintOutputCommand));
			TakeInputCommand = ReactiveCommand.Create(TakeInput, this.WhenAnyValue(x => x.Input, x => x.Length > 0));
			OpenMenuCommand = ReactiveCommand.Create<string>(OpenMenu);
			DisconnectCommand = ReactiveCommand.CreateFromTask<Window>(DisconnectAsync);
			RestartCommand = ReactiveCommand.CreateFromTask<Window>(RestartAsync);
			PauseCommand = ReactiveCommand.Create(Pause);
			OpenFileSearchWindowCommand = ReactiveCommand.CreateFromTask<Window>(OpenFileSearchWindowAsync);
			SaveColorsCommand = ReactiveCommand.Create(SaveColorSettings,
				this.WhenAnyValue(x => x.OpenColorsMenu, x => x.ColorsViewModel.CanSave, (o, c) => o && c));
			SaveBotSettingsCommand = ReactiveCommand.Create(SaveBotSettings,
				this.WhenAnyValue(x => x.OpenSettingsMenu, x => x.BotSettingsViewModel.CanSave, (o, c) => o && c));
			ClearOutputCommand = ReactiveCommand.CreateFromTask<Window>(ClearOutput);
			SaveOutputCommand = ReactiveCommand.Create(SaveOutput);
			OpenOutputSearchWindowCommand = ReactiveCommand.CreateFromTask<Window>(OpenOutputSearchWindowAsync);

			var timer = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1));
			Uptime = timer.Select(_ => $"Uptime: {ProcessInfoUtils.GetUptime():dd\\.hh\\:mm\\:ss}");
			Latency = timer.Select(_ => $"Latency: {(_Client?.CurrentUser == null ? -1 : _Client.Latency)}ms");
			Memory = timer.Select(_ => $"Memory: {ProcessInfoUtils.GetMemoryMB():0.00}MB");
			ThreadCount = timer.Select(_ => $"Threads: {ProcessInfoUtils.GetThreadCount()}");
		}

		public BotSettingsViewModel BotSettingsViewModel { get; }

		public ICommand ClearOutputCommand { get; }

		public ColorsViewModel ColorsViewModel { get; }

		public ICommand DisconnectCommand { get; }

		public string Input
		{
			get => _Input;
			set => this.RaiseAndSetIfChanged(ref _Input, value);
		}

		public IObservable<string> Latency { get; }

		public LogServiceViewModel LogServiceViewModel { get; }

		public string MainMenuText { get; } =
																																					"Latency: Time it takes for a command to reach the bot.\n\n" +
			"Memory: Amount of RAM the program is using.\n\n" +
			"Threads: Where all the actions in the bot happen.\n\n" +
			$"API wrapper version: {Constants.API_VERSION}\n" +
			$"Bot version: {Constants.BOT_VERSION}\n\n" +
			$"Github repository for Advobot: {Constants.REPO}\n" +
			$"Join the Discord server for additional help: {Constants.DISCORD_INV}";

		public IObservable<string> Memory { get; }

		public bool OpenColorsMenu => GetMenuStatus();

		public ICommand OpenFileSearchWindowCommand { get; }

		public bool OpenInfoMenu => GetMenuStatus();

		public bool OpenMainMenu => GetMenuStatus();

		public ICommand OpenMenuCommand { get; }

		public ICommand OpenOutputSearchWindowCommand { get; }

		public bool OpenSettingsMenu => GetMenuStatus();

		public string Output
		{
			get => _Output;
			set => this.RaiseAndSetIfChanged(ref _Output, value);
		}

		public int OutputColumnSpan
		{
			get => _OutputColumnSpan;
			private set => this.RaiseAndSetIfChanged(ref _OutputColumnSpan, value);
		}

		public string PauseButtonContent
		{
			get => _PauseButtonContent;
			set => this.RaiseAndSetIfChanged(ref _PauseButtonContent, value);
		}

		public ICommand PauseCommand { get; }
		public ICommand PrintOutputCommand { get; }
		public ICommand RestartCommand { get; }
		public ICommand SaveBotSettingsCommand { get; }
		public ICommand SaveColorsCommand { get; }
		public ICommand SaveOutputCommand { get; }
		public ICommand TakeInputCommand { get; }
		public IObservable<string> ThreadCount { get; }
		public IObservable<string> Uptime { get; }

		private async Task ClearOutput(Window window)
		{
			if (await MessageBox.ShowAsync(window, "Are you sure you want to clear the output window?", _Caption, new[] { "Yes", "No" }) == "Yes")
			{
				Output = "";
			}
		}

		private async Task DisconnectAsync(Window window)
		{
			if (await MessageBox.ShowAsync(window, "Are you sure you want to disconnect the bot?", _Caption, new[] { "Yes", "No" }) == "Yes")
			{
				await _Client.DisconnectBotAsync();
			}
		}

		private bool GetMenuStatus([CallerMemberName] string caller = "")
							=> _MenuStatuses.GetOrAdd(caller, false);

		private async Task OpenFileSearchWindowAsync(Window window)
		{
			//Returns array of strings, but AllowMultiple is false so should only have 1 or 0
			var file = (await new OpenFileDialog
			{
				InitialDirectory = _BotSettings.BaseBotDirectory.FullName,
				Title = "Advobot - File Search",
				AllowMultiple = false,
			}.ShowAsync(window)).SingleOrDefault();
			if (file != null)
			{
				await new FileViewingWindow
				{
					DataContext = new FileViewingWindowViewModel(new FileInfo(file), null),
				}.ShowDialog(window);
			}
		}

		private void OpenMenu(string name)
		{
			foreach (var key in new List<string>(_MenuStatuses.Keys))
			{
				//If not the targeted menu, set to false
				//If the targeted menu, toggle the visibility
				var currentValue = _MenuStatuses[key];
				var newValue = key == name && !currentValue;
				if (currentValue != newValue)
				{
					_MenuStatuses[key] = newValue;
					this.RaisePropertyChanged(key);
				}
			}
			OutputColumnSpan = _MenuStatuses.Any(kvp => kvp.Value) ? 1 : 2;
		}

		private Task OpenOutputSearchWindowAsync(Window window)
			=> new OutputSearchWindow { DataContext = new OutputSearchWindowViewModel(_BotSettings), }.ShowDialog(window);

		private void Pause()
		{
			PauseButtonContent = _BotSettings.Pause ? "Pause" : "Unpause";
			ConsoleUtils.WriteLine($"The bot is now {(_BotSettings.Pause ? "unpaused" : "paused")}.", name: "Pause");
			_BotSettings.Pause = !_BotSettings.Pause;
		}

		private void PrintOutput(string value)
											=> Output += value;

		private async Task RestartAsync(Window window)
		{
			if (await MessageBox.ShowAsync(window, "Are you sure you want to restart the bot?", _Caption, new[] { "Yes", "No" }) == "Yes")
			{
				await _Client.RestartBotAsync(_BotSettings);
			}
		}

		private void SaveBotSettings()
		{
			_BotSettings.Save();
			ConsoleUtils.WriteLine("Successfully saved the bot settings.", name: "Saving");
		}

		private void SaveColorSettings()
		{
			_Colors.Save();
			ConsoleUtils.WriteLine("Successfully saved the color settings.", name: "Saving");
		}

		private void SaveOutput()
		{
			var response = _BotSettings.GenerateFileName("Output").SaveAndGetResponse(Output).Text;
			ConsoleUtils.WriteLine(response, name: "Saving Output");
		}

		private void TakeInput()
		{
			ConsoleUtils.WriteLine(Input, name: "UIInput");
			Input = "";
		}
	}
}