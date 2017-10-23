using Advobot.Core.Actions;
using Advobot.Core.Interfaces;
using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Enums;
using Discord;
using ICSharpCode.AvalonEdit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Advobot.UILauncher.Classes.AdvobotWindow
{
	public partial class AdvobotWindow : Window
	{
		public AdvobotWindow()
		{
			FontFamily = new FontFamily("Courier New");
			InitializeComponents();

			Console.SetOut(new TextBoxStreamWriter(_Output));

			new ColorSettings().ActivateTheme();
			ColorSettings.SwitchElementColorOfChildren(_Layout);

			Loaded += AttemptToLogin;
			_UpdateTimer.Tick += UpdateMenus;
		}

		private static bool _StartUp = true;
		private static bool _GotPath;
		private static bool _GotKey;
		private async Task HandleInput(string input)
		{
			if (!_GotPath)
			{
				var provider = await UIBotWindowLogic.GetPath(input, _StartUp);
				if (provider != null)
				{
					_StartUp = true;
					_GotPath = true;
					_Client = provider.GetService<IDiscordClient>();
					_BotSettings = provider.GetService<IBotSettings>();
					_Logging = provider.GetService<ILogService>();
					_UISettings = ColorSettings.LoadUISettings(_StartUp);
					_UISettings.ActivateTheme();
					ColorSettings.SwitchElementColorOfChildren(_Layout);
				}
				else
				{
					_StartUp = false;
				}
			}
			else if (!_GotKey)
			{
				if (await Config.ValidateBotKey(_Client, input, _StartUp))
				{
					_StartUp = true;
					_GotKey = true;
				}
				else
				{
					_StartUp = false;
				}
			}

			if (!_GotKey && _StartUp)
			{
				if (await Config.ValidateBotKey(_Client, null, _StartUp))
				{
					_StartUp = true;
					_GotKey = true;
				}
				else
				{
					_StartUp = false;
				}
			}

			if (_GotPath && _GotKey && _StartUp)
			{
				_UpdateTimer.Start();
				await ClientActions.StartAsync(_Client);
				_StartUp = false;
			}
		}

		private async Task UpdateSettingsWhenOpened()
		{
			((CheckBox)((Viewbox)_DownloadUsersSetting.Setting).Child).IsChecked = _BotSettings.AlwaysDownloadUsers;
			((TextBox)_PrefixSetting.Setting).Text = _BotSettings.Prefix;
			((TextBox)_GameSetting.Setting).Text = _BotSettings.Game;
			((TextBox)_StreamSetting.Setting).Text = _BotSettings.Stream;
			((TextBox)_ShardSetting.Setting).Text = _BotSettings.ShardCount.ToString();
			((TextBox)_MessageCacheSetting.Setting).Text = _BotSettings.MessageCacheCount.ToString();
			((TextBox)_UserGatherCountSetting.Setting).Text = _BotSettings.MaxUserGatherCount.ToString();
			((TextBox)_MessageGatherSizeSetting.Setting).Text = _BotSettings.MaxMessageGatherSize.ToString();
			((ComboBox)_LogLevelComboBox.Setting).SelectedItem = ((ComboBox)_LogLevelComboBox.Setting).Items.OfType<TextBox>().FirstOrDefault(x => (LogSeverity)x.Tag == _BotSettings.LogLevel);
			_TrustedUsersComboBox.ItemsSource = await Task.WhenAll(_BotSettings.TrustedUsers.Select(async x => AdvobotTextBox.CreateUserBox(await _Client.GetUserAsync(x))));
		}
	}
}