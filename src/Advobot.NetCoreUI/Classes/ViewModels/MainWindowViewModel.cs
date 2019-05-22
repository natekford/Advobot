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
using Advobot.Interfaces;
using Advobot.NetCoreUI.Classes.AbstractUI.Colors;
using Advobot.NetCoreUI.Classes.Colors;
using Advobot.NetCoreUI.Classes.Views;
using Advobot.NetCoreUI.Utils;
using Advobot.Utilities;
using AdvorangesUtils;
using Avalonia.Controls;
using Avalonia.Media;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace Advobot.NetCoreUI.Classes.ViewModels
{
	public sealed class AdvobotNetCoreWindowViewModel : ReactiveObject
	{
		private static readonly string _Caption = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;

		public string MainMenuText { get; } =
			"Latency: Time it takes for a command to reach the bot.\n\n" +
			"Memory: Amount of RAM the program is using.\n\n" +
			"Threads: Where all the actions in the bot happen.\n\n" +
			$"API wrapper version: {Constants.API_VERSION}\n" +
			$"Bot version: {Constants.BOT_VERSION}\n\n" +
			$"Github repository for Advobot: {Constants.REPO}\n" +
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
			set => this.RaiseAndSetIfChanged(ref _Input, value);
		}
		private string _Input = "";

		public string PauseButtonContent
		{
			get => _PauseButtonContent;
			set => this.RaiseAndSetIfChanged(ref _PauseButtonContent, value);
		}
		private string _PauseButtonContent = "";

		public int OutputColumnSpan
		{
			get => _OutputColumnSpan;
			private set => this.RaiseAndSetIfChanged(ref _OutputColumnSpan, value);
		}
		private int _OutputColumnSpan = 2;

		public LogServiceViewModel LogServiceViewModel { get; }
		public BotSettingsViewModel BotSettingsViewModel { get; }
		public ColorsViewModel ColorsViewModel { get; }

		public bool OpenMainMenu => GetMenuStatus();
		public bool OpenInfoMenu => GetMenuStatus();
		public bool OpenColorsMenu => GetMenuStatus();
		public bool OpenSettingsMenu => GetMenuStatus();
		private readonly ConcurrentDictionary<string, bool> _MenuStatuses = new ConcurrentDictionary<string, bool>();

		public IObservable<string> Uptime { get; }
		public IObservable<string> Latency { get; }
		public IObservable<string> Memory { get; }
		public IObservable<string> ThreadCount { get; }

		public ICommand PrintOutputCommand { get; }
		public ICommand TakeInputCommand { get; }
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

		private readonly BaseSocketClient _Client;
		private readonly ILogService _LogService;
		private readonly IBotSettings _BotSettings;
		private readonly IColorSettings<ISolidColorBrush> _Colors;

		public AdvobotNetCoreWindowViewModel(IServiceProvider provider)
		{
			_Client = provider.GetRequiredService<BaseSocketClient>();
			_LogService = provider.GetRequiredService<ILogService>();
			_BotSettings = provider.GetRequiredService<IBotSettings>();
			_Colors = NetCoreColorSettings.Load<NetCoreColorSettings>(_BotSettings) ?? new NetCoreColorSettings();

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
			Uptime = timer.Select(x => $"Uptime: {ProcessInfoUtils.GetUptime():dd\\.hh\\:mm\\:ss}");
			Latency = timer.Select(x => $"Latency: {(_Client?.CurrentUser == null ? -1 : _Client.Latency)}ms");
			Memory = timer.Select(x => $"Memory: {ProcessInfoUtils.GetMemoryMB():0.00}MB");
			ThreadCount = timer.Select(x => $"Threads: {ProcessInfoUtils.GetThreadCount()}");
		}

		private bool GetMenuStatus([CallerMemberName] string caller = "")
			=> _MenuStatuses.GetOrAdd(caller, false);
		private void PrintOutput(string value)
			=> Output += value;
		private void TakeInput()
		{
			ConsoleUtils.WriteLine(Input, name: "UIInput");
			Input = "";
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
		private async Task DisconnectAsync(Window window)
		{
			if (await MessageBox.ShowAsync(window, "Are you sure you want to disconnect the bot?", _Caption, new[] { "Yes", "No" }) == "Yes")
			{
				await _Client.DisconnectBotAsync();
			}
		}
		private async Task RestartAsync(Window window)
		{
			if (await MessageBox.ShowAsync(window, "Are you sure you want to restart the bot?", _Caption, new[] { "Yes", "No" }) == "Yes")
			{
				await _Client.RestartBotAsync(_BotSettings);
			}
		}
		private void Pause()
		{
			PauseButtonContent = _BotSettings.Pause ? "Pause" : "Unpause";
			ConsoleUtils.WriteLine($"The bot is now {(_BotSettings.Pause ? "unpaused" : "paused")}.", name: "Pause");
			_BotSettings.Pause = !_BotSettings.Pause;
		}
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
		private void SaveColorSettings()
		{
			ConsoleUtils.WriteLine("Successfully saved the color settings.", name: "Saving");
			_Colors.Save(_BotSettings);
		}
		private void SaveBotSettings()
		{
			ConsoleUtils.WriteLine("Successfully saved the bot settings.", name: "Saving");
			_BotSettings.SaveSettings();
		}
		private async Task ClearOutput(Window window)
		{
			if (await MessageBox.ShowAsync(window, "Are you sure you want to clear the output window?", _Caption, new[] { "Yes", "No" }) == "Yes")
			{
				Output = "";
			}
		}
		private void SaveOutput()
		{
			var response = _BotSettings.GenerateFileName("Output").SaveAndGetResponse(Output).Text;
			ConsoleUtils.WriteLine(response, name: "Saving Output");
		}
		private async Task OpenOutputSearchWindowAsync(Window window)
			=> await new OutputSearchWindow { DataContext = new OutputSearchWindowViewModel(_BotSettings), }.ShowDialog(window);
	}
}