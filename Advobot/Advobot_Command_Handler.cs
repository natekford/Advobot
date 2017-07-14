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
		private static IDiscordClient Client;
		private static CommandService Commands;
		private static BotGlobalInfo BotInfo;
		private static LogHolder Logging;

		public static async Task Install(IServiceProvider provider)
		{
			Provider = provider;
			Client = (IDiscordClient)provider.GetService(typeof(IDiscordClient));
			Commands = (CommandService)provider.GetService(typeof(CommandService));
			BotInfo = (BotGlobalInfo)provider.GetService(typeof(BotGlobalInfo));
			Logging = (LogHolder)provider.GetService(typeof(LogHolder));

			Commands.AddTypeReader(typeof(IInvite), new IInviteTypeReader());
			Commands.AddTypeReader(typeof(IBan), new IBanTypeReader());
			Commands.AddTypeReader(typeof(Emote), new IEmoteTypeReader());
			Commands.AddTypeReader(typeof(Color), new ColorTypeReader());
			await Commands.AddModulesAsync(System.Reflection.Assembly.GetEntryAssembly());
		}

		public static async Task LoadInformation()
		{
			await SavingAndLoading.LoadInformation(Client, BotInfo);
		}

		public static async Task HandleCommand(SocketUserMessage message)
		{
			if (BotInfo.Pause)
				return;

			var guild = (message?.Channel as SocketTextChannel)?.Guild;
			if (guild == null)
				return;

			var guildInfo = await SavingAndLoading.CreateOrGetGuildInfo(guild);
			if (!TryGetArgPos(message, ((string)guildInfo.GetSetting(SettingOnGuild.Prefix)), out int argPos))
				return;

			var context = new MyCommandContext(BotInfo, guildInfo, Client, message);
			var result = await Commands.ExecuteAsync(context, argPos, Provider);
			if (result.IsSuccess)
			{
				await Logging.ModLog.LogCommand(context);
			}
			else if (Constants.IGNORE_ERROR.CaseInsEquals(result.ErrorReason))
			{
				return;
			}
			else
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
						Messages.WriteLine(result.ErrorReason);
						return;
					}
					default:
					{
						await Messages.MakeAndDeleteSecondaryMessage(message.Channel, message, Formatting.ERROR(result.ErrorReason));
						return;
					}
				}
			}
		}

		private static bool TryGetArgPos(IUserMessage message, string guildPrefix, out int argPos)
		{
			argPos = -1;
			if (String.IsNullOrWhiteSpace(guildPrefix))
			{
				var globalPrefix = ((string)BotInfo.GetSetting(SettingOnBot.Prefix));
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