using Advobot.Commands;
using Advobot.Core;
using Advobot.Core.Actions;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Advobot.UILauncher.Classes
{
	internal class LoginHandler : IServiceProvider
	{
		private IServiceProvider _Provider;
		private bool _StartUp = true;
		public bool GotPath { get; private set; }
		public bool GotKey { get; private set; }
		public bool CanLogin { get; private set; }
		public event RoutedEventHandler AbleToStart;

		public async Task AttemptToStart(string input)
		{
			if (!GotPath)
			{
				//Null means it's from the loaded event, which is start up so it's telling the bot to look up the config value
				_StartUp = input == null;
				//Set startup to whatever returned value is so it can be used in GotKey, and then after GotKey in the last if statement
				_StartUp = GotPath = (_Provider = await GetPath(input, _StartUp)) != null;
			}
			else if (!GotKey)
			{
				_StartUp = input == null;
				_StartUp = GotKey = await Config.ValidateBotKey(_Provider.GetRequiredService<IDiscordClient>(), input, _StartUp);
			}

			if (_StartUp && (CanLogin = GotKey && GotPath))
			{
				_StartUp = false;
				AbleToStart?.Invoke(this, new RoutedEventArgs());
			}
		}
		private static async Task<IServiceProvider> GetPath(string path, bool startup)
		{
			if (Config.ValidatePath(path, startup))
			{
				var provider = await CreationActions.CreateServiceProvider().CAF();
				CommandHandler.Install(provider);
				return provider;
			}
			return null;
		}
		private static async Task<bool> GetKey(IDiscordClient client, string key, bool startup)
		{
			return await Config.ValidateBotKey(client, key, startup);
		}

		public object GetService(Type serviceType)
		{
			return _Provider.GetService(serviceType);
		}
	}
}
