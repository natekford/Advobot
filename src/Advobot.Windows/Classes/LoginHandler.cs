using Advobot;
using Advobot.Classes;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Advobot.Windows.Classes
{
	internal class LoginHandler
	{
		public IServiceProvider Provider { get; private set; }
		public CommandHandler CommandHandler { get; private set; }

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
			if (!GotPath)
			{
				//Null means it's from the loaded event, which is start up so it's telling the bot to look up the config value
				_StartUp = input == null;
				//Set startup to whatever returned value is so it can be used in GotKey, and then after GotKey in the last if statement
				_StartUp = GotPath = GetPath(input, _StartUp);
			}
			else if (!GotKey)
			{
				_StartUp = input == null;
				_StartUp = GotKey = await GetKey(Provider.GetRequiredService<IDiscordClient>(), input, _StartUp).CAF();
			}

			var somethingWasSet = _StartUp;
			if (_StartUp && (CanLogin = GotKey && GotPath))
			{
				_StartUp = false;
				AbleToStart?.Invoke();
			}
			return somethingWasSet;
		}
		private bool GetPath(string path, bool startup)
		{
			if (LowLevelConfig.Config.ValidatePath(path, startup))
			{
				Provider = CreationUtils.CreateDefaultServiceProvider<BotSettings, GuildSettings>(DiscordUtils.GetCommandAssemblies());
				CommandHandler = new CommandHandler(Provider);
				return true;
			}
			return false;
		}
		private async Task<bool> GetKey(IDiscordClient client, string key, bool startup)
		{
			return await LowLevelConfig.Config.ValidateBotKey(client, key, startup);
		}
	}
}
