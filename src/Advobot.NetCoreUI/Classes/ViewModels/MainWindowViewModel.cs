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
using Advobot.NetCoreUI.Classes.Colors;
using Advobot.NetCoreUI.Classes.Views;
using Advobot.NetCoreUI.Utils;
using Advobot.SharedUI.Colors;
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
		private ConcurrentDictionary<string, bool> OpenMenus = new ConcurrentDictionary<string, bool>();

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

		private readonly DiscordShardedClient _Client;
		private readonly ILogService _LogService;
		private readonly IBotSettings _BotSettings;
		private readonly IGuildSettingsFactory _GuildSettings;
		private readonly IColorSettings<ISolidColorBrush> _Colors;

		public AdvobotNetCoreWindowViewModel(IServiceProvider provider)
		{
			_Client = provider.GetRequiredService<DiscordShardedClient>();
			_LogService = provider.GetRequiredService<ILogService>();
			_BotSettings = provider.GetRequiredService<IBotSettings>();
			_GuildSettings = provider.GetRequiredService<IGuildSettingsFactory>();
			_Colors = NetCoreColorSettings.Load<NetCoreColorSettings>(_BotSettings);

			LogServiceViewModel = new LogServiceViewModel(_LogService);
			BotSettingsViewModel = new BotSettingsViewModel(_BotSettings);
			ColorsViewModel = new ColorsViewModel(_Colors);

			PrintOutputCommand = ReactiveCommand.Create<string>(PrintOutput);
			Console.SetOut(new TextBoxStreamWriter(PrintOutputCommand));
			TakeInputCommand = ReactiveCommand.Create(TakeInput, this.WhenAnyValue(x => x.CanInput));
			OpenMenuCommand = ReactiveCommand.Create<string>(OpenMenu);
			DisconnectCommand = ReactiveCommand.CreateFromTask(DisconnectAsync);
			RestartCommand = ReactiveCommand.CreateFromTask(RestartAsync);
			PauseCommand = ReactiveCommand.Create(Pause);
			OpenFileSearchWindowCommand = ReactiveCommand.CreateFromTask<Window>(OpenFileSearchWindowAsync);
			SaveColorsCommand = ReactiveCommand.Create(SaveColorSettings, this.WhenAnyValue(x => x.OpenColorsMenu));
			SaveBotSettingsCommand = ReactiveCommand.Create(SaveBotSettings, this.WhenAnyValue(x => x.OpenSettingsMenu));
			ClearOutputCommand = ReactiveCommand.CreateFromTask(ClearOutput);
			SaveOutputCommand = ReactiveCommand.Create(SaveOutput);
			OpenOutputSearchWindowCommand = ReactiveCommand.CreateFromTask(OpenOutputSearchWindowAsync);

			var timer = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1));
			Uptime = timer.Select(x => $"Uptime: {ProcessInfoUtils.GetUptime():dd\\.hh\\:mm\\:ss}");
			Latency = timer.Select(x => $"Latency: {(_Client?.CurrentUser == null ? -1 : _Client.Latency)}ms");
			Memory = timer.Select(x => $"Memory: {ProcessInfoUtils.GetMemoryMB():0.00}MB");
			ThreadCount = timer.Select(x => $"Threads: {ProcessInfoUtils.GetThreadCount()}");
		}

		private bool GetMenuStatus([CallerMemberName] string menu = "")
			=> OpenMenus.GetOrAdd(menu, false);
		private void PrintOutput(string value) => Output += value;
		private void TakeInput()
		{
			ConsoleUtils.WriteLine(Input, name: "UIInput");
			Input = "";
		}
		private void OpenMenu(string name)
		{
			foreach (var key in new List<string>(OpenMenus.Keys))
			{
				//If not the targeted menu, set to false
				//If the targeted menu, toggle the visibility
				var currentValue = OpenMenus[key];
				var newValue = key == name && !currentValue;
				if (currentValue != newValue)
				{
					OpenMenus[key] = newValue;
					this.RaisePropertyChanged(key);
				}
			}
			OutputColumnSpan = OpenMenus.Any(kvp => kvp.Value) ? 1 : 2;
		}
		private async Task DisconnectAsync()
		{
			if (await MessageBox.Show("Are you sure you want to disconnect the bot?", _Caption, new[] { "Yes", "No" }) == "Yes")
			{
				await ClientUtils.DisconnectBotAsync(_Client);
			}
		}
		private async Task RestartAsync()
		{
			if (await MessageBox.Show("Are you sure you want to restart the bot?", _Caption, new[] { "Yes", "No" }) == "Yes")
			{
				await ClientUtils.RestartBotAsync(_Client, _BotSettings);
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
			//Returns array of strings, but AllowMultiple is false so should only have 1
			var files = await new OpenFileDialog
			{
				InitialDirectory = _BotSettings.BaseBotDirectory.FullName,
				Title = "Advobot - File Search",
			}.ShowAsync(window);
			if (files.Any())
			{
				var file = files[0];
				var type = GetDeserializationType(file);
				await new FileViewingWindow { DataContext = new FileViewingWindowViewModel(new FileInfo(file), type), }.ShowDialog();
			}
		}
		private void SaveColorSettings()
		{
			ConsoleUtils.WriteLine("Successfully saved the color settings.", name: "Saving");
			_Colors.SaveSettings(_BotSettings);
		}
		private void SaveBotSettings()
		{
			ConsoleUtils.WriteLine("Successfully saved the bot settings.", name: "Saving");
			_BotSettings.SaveSettings();
		}
		private async Task ClearOutput()
		{
			if (await MessageBox.Show("Are you sure you want to clear the output window?", _Caption, new[] { "Yes", "No" }) == "Yes")
			{
				Output = "";
			}
		}
		private void SaveOutput()
		{
			var response = _BotSettings.GenerateFileName("Output").SaveAndGetResponse(Output);
			ConsoleUtils.WriteLine(response, name: "Saving Output");
		}
		private Task OpenOutputSearchWindowAsync()
			=> new OutputSearchWindow { DataContext = new OutputSearchWindowViewModel(_BotSettings), }.ShowDialog();

		private Type GetDeserializationType(string fileName)
		{
			switch (fileName)
			{
				case string str when (str == _BotSettings.GetFile().FullName):
					return _BotSettings.GetType();
				case string str when (str == _Colors.GetFile(_BotSettings).FullName):
					return _Colors.GetType();
				case string str when (Path.GetDirectoryName(str) == _GuildSettings.GetDirectory(_BotSettings).FullName):
					return _BotSettings.GetType();
				default:
					return null;
			}
		}
	}
}