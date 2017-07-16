using Advobot.Actions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Advobot.Logging;

namespace Advobot
{
	public class CommandHandler
	{
		private static IServiceProvider Provider;
		private static CommandService Commands;
		private static IGlobalSettings BotInfo;
		private static IDiscordClient Client;
		private static ILogModule Logging;
		private static IGuildSettingsModule GuildSettings;

		public static async Task Install(IServiceProvider provider)
		{
			Provider = provider;
			Commands = (CommandService)provider.GetService(typeof(CommandService));
			BotInfo = (IGlobalSettings)provider.GetService(typeof(IGlobalSettings));
			Client = (IDiscordClient)provider.GetService(typeof(IDiscordClient));
			Logging = (ILogModule)provider.GetService(typeof(ILogModule));
			GuildSettings = (IGuildSettingsModule)provider.GetService(typeof(IGuildSettingsModule));

			Commands.AddTypeReader(typeof(IInvite), new IInviteTypeReader());
			Commands.AddTypeReader(typeof(IBan), new IBanTypeReader());
			Commands.AddTypeReader(typeof(Emote), new IEmoteTypeReader());
			Commands.AddTypeReader(typeof(Color), new ColorTypeReader());
			await Commands.AddModulesAsync(System.Reflection.Assembly.GetEntryAssembly());
		}

		public static async Task LoadInformation()
		{
			await SavingAndLoading.LoadInformation(Client, BotInfo, GuildSettings);
		}

		public static async Task HandleCommand(SocketUserMessage message)
		{
			if (BotInfo.Pause)
				return;

			var guild = (message?.Channel as SocketTextChannel)?.Guild;
			if (guild == null)
				return;

			if (!GuildSettings.TryGetSettings(guild, out IGuildSettings guildInfo))
			{
				await GuildSettings.AddGuild(guild);
				guildInfo = GuildSettings.GetSettings(guild);
			}
			if (!TryGetArgPos(message, guildInfo.Prefix, BotInfo.Prefix, out int argPos))
				return;

			var context = new MyCommandContext(BotInfo, guildInfo, Logging, Client, message);
			var result = await Commands.ExecuteAsync(context, argPos, Provider);

			if (result.IsSuccess)
			{
				await (Logging.ModLog as ModLogger).LogCommand(context);

				Logging.IncrementSuccessfulCommands();
			}
			else if (!Constants.IGNORE_ERROR.CaseInsEquals(result.ErrorReason))
			{
				//Ignore commands with the unknown command error because it's annoying
				switch (result.Error)
				{
					case CommandError.UnknownCommand:
					{
						return;
					}
					case CommandError.Exception:
					{
						ConsoleActions.WriteLine(result.ErrorReason);
						break;
					}
					default:
					{
						await Messages.MakeAndDeleteSecondaryMessage(message.Channel, message, Formatting.ERROR(result.ErrorReason));
						break;
					}
				}

				Logging.IncrementFailedCommands();
			}
		}

		private static bool TryGetArgPos(IUserMessage message, string guildPrefix, string globalPrefix, out int argPos)
		{
			argPos = -1;
			if (String.IsNullOrWhiteSpace(guildPrefix))
			{
				if (message.HasStringPrefix(globalPrefix, ref argPos))
				{
					return true;
				}
			}
			else
			{
				if (message.HasStringPrefix(guildPrefix, ref argPos) || message.HasMentionPrefix(Client.CurrentUser, ref argPos))
				{
					return true;
				}
			}
			return false;
		}
	}
}