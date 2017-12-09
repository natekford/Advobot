using Advobot.Commands;
using Advobot.Core;
using Advobot.Core.Actions;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Advobot.UILauncher.Classes
{
	internal class LoginHandler : IServiceProvider
	{
		private IServiceProvider _Provider;
		private bool _StartUp = true;
		public bool GotPath { get; private set; }
		public bool GotKey { get; private set; }
		public bool CanLogin { get; private set; }
		public event Func<Task> AbleToStart;

		/// <summary>
		/// Attempts to first set the save path, then the bot's key. Returns true if either get set.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public async Task<bool> AttemptToStart(string input)
		{
			if (!this.GotPath)
			{
				//Null means it's from the loaded event, which is start up so it's telling the bot to look up the config value
				this._StartUp = input == null;
				//Set startup to whatever returned value is so it can be used in GotKey, and then after GotKey in the last if statement
				this._StartUp = this.GotPath = (this._Provider = await GetPath(input, this._StartUp)) != null;
			}
			else if (!this.GotKey)
			{
				this._StartUp = input == null;
				this._StartUp = this.GotKey = await Config.ValidateBotKey(this._Provider.GetRequiredService<IDiscordClient>(), input, this._StartUp);
			}

			var somethingWasSet = this._StartUp;
			if (this._StartUp && (this.CanLogin = this.GotKey && this.GotPath))
			{
				this._StartUp = false;
				AbleToStart?.Invoke();
			}
			return somethingWasSet;
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
			=> await Config.ValidateBotKey(client, key, startup);

		public object GetService(Type serviceType) => this._Provider.GetService(serviceType);
	}
}
