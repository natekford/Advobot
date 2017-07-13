using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Advobot
{
	public class Command_Handler
	{
		private static CommandService Commands;
		private static BotClient Client;
		private static IServiceProvider Provider;

		public async Task Install(IServiceProvider provider)
		{
			Client = (BotClient)provider.GetService(typeof(BotClient));
			Commands = (CommandService)provider.GetService(typeof(CommandService));
			Provider = provider;

			Commands.AddTypeReader(typeof(IInvite), new IInviteTypeReader());
			Commands.AddTypeReader(typeof(IBan), new IBanTypeReader());
			Commands.AddTypeReader(typeof(Emote), new IEmoteTypeReader());
			Commands.AddTypeReader(typeof(Color), new ColorTypeReader());
			await Commands.AddModulesAsync(System.Reflection.Assembly.GetEntryAssembly());
		}

		public static async Task HandleCommand(SocketUserMessage message)
		{
			if (Variables.Pause)
				return;

			var guild = (message?.Channel as SocketTextChannel)?.Guild;
			if (guild == null)
				return;

			var guildInfo = await Actions.CreateOrGetGuildInfo(guild);
			if (!TryGetArgPos(message, ((string)guildInfo.GetSetting(SettingOnGuild.Prefix)), out int argPos))
				return;

			var context = new MyCommandContext(guildInfo, (DiscordSocketClient)Client.GetClient(), message);
			var result = await Commands.ExecuteAsync(context, argPos, Provider);
			if (result.IsSuccess)
			{
				await Mod_Logs.LogCommand(context);
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
						Actions.WriteLine(result.ErrorReason);
						return;
					}
					default:
					{
						await Actions.MakeAndDeleteSecondaryMessage(message.Channel, message, Actions.ERROR(result.ErrorReason));
						return;
					}
				}
			}
		}

		public static bool TryGetArgPos(IUserMessage message, string guildPrefix, out int argPos)
		{
			argPos = -1;
			if (String.IsNullOrWhiteSpace(guildPrefix))
			{
				var globalPrefix = ((string)Variables.BotInfo.GetSetting(SettingOnBot.Prefix));
				if (message.HasStringPrefix(globalPrefix, ref argPos))
				{
					return true;
				}
			}
			else
			{
				if (message.HasStringPrefix(guildPrefix, ref argPos) || message.HasMentionPrefix(Variables.Client.GetCurrentUser(), ref argPos))
				{
					return true;
				}
			}
			return false;
		}
	}
}