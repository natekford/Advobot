using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Advobot
{
	public class Command_Handler
	{
		private static CommandService Commands;
		private static BotClient Client;
		private static IServiceProvider Provider;

		public async Task Install(IServiceProvider provider)
		{
			//Create Command Service, inject it into Dependency Map
			Client = (BotClient)provider.GetService(typeof(BotClient));
			Commands = (CommandService)provider.GetService(typeof(CommandService));
			Provider = provider;

			Commands.AddTypeReader(typeof(IInvite), new IInviteTypeReader());
			Commands.AddTypeReader(typeof(IBan), new IBanTypeReader());
			await Commands.AddModulesAsync(Assembly.GetEntryAssembly());
		}

		public static async Task HandleCommand(SocketUserMessage message)
		{
			if (Variables.Pause)
				return;
			var guild = (message?.Channel as SocketTextChannel)?.Guild;
			if (guild == null)
				return;

			var guildInfo = await Actions.CreateOrGetGuildInfo(guild);
			if (!PrefixHandling(message, ((string)guildInfo.GetSetting(SettingOnGuild.Prefix)), out int argPos))
				return;

			var context = new MyCommandContext(guildInfo, Client.GetClient(), message);
			if (!await ValidateCommand(context, argPos))
				return;
			
			//Ignore unknown command errors because they're annoying and ignore the errors given by lack of permissions, etc. put in by me
			var result = await Commands.ExecuteAsync(context, argPos, Provider);
			if (result.IsSuccess)
			{
				await Mod_Logs.LogCommand(guildInfo, context);
				await Actions.DeleteMessage(message);
			}
			else if (Actions.CaseInsEquals(result.ErrorReason, Constants.IGNORE_ERROR) || result.Error == CommandError.UnknownCommand)
			{
				return;
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(context, Actions.ERROR(result.ErrorReason));
			}
		}

		public static bool PrefixHandling(IUserMessage message, string guildPrefix, out int argPos)
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
				if (message.HasStringPrefix(guildPrefix, ref argPos))
				{
					return true;
				}
				else if (message.HasMentionPrefix(Variables.Client.GetCurrentUser(), ref argPos))
				{
					return true;
				}
			}
			return false;
		}

		public static bool CheckIfCommandEnabled(MyCommandContext context, int argPos)
		{
			if (context.Guild == null)
				return false;

			//Get the command
			var cmd = Actions.GetCommand(context.GuildInfo, context.Message.Content.Substring(argPos).Split(' ').FirstOrDefault());
			if (cmd == null)
			{
				return false;
			}
			else if (!cmd.ValAsBoolean)
			{
				return false;
			}

			/* I'm not sure exactly how I want this permission system set up.
			 * I think I want it to be like this:
			 * If user is set, use user setting
			 * Else if any roles are set, use the highest role setting
			 * Else if channel is set, use channel setting
			 */

			var user = ((List<CommandOverride>)context.GuildInfo.GetSetting(SettingOnGuild.CommandsDisabledOnUser)).FirstOrDefault(x =>
			{
				return true
				&& Actions.CaseInsEquals(cmd.Name, x.Name)
				&& x.ID == context.User.Id;
			});
			if (user != null)
			{
				return user.Enabled;
			}

			var role = ((List<CommandOverride>)context.GuildInfo.GetSetting(SettingOnGuild.CommandsDisabledOnRole)).Where(x =>
			{
				return true
				&& Actions.CaseInsEquals(cmd.Name, x.Name)
				&& (context.User as IGuildUser).RoleIds.Contains(x.ID);
			}).OrderBy(x =>
			{
				return context.Guild.GetRole(x.ID).Position;
			}).LastOrDefault();
			if (role != null)
			{
				return role.Enabled;
			}

			var channel = ((List<CommandOverride>)context.GuildInfo.GetSetting(SettingOnGuild.CommandsDisabledOnChannel)).FirstOrDefault(x =>
			{
				return true
				&& Actions.CaseInsEquals(cmd.Name, x.Name)
				&& x.ID == context.Channel.Id;
			});
			if (channel != null)
			{
				return channel.Enabled;
			}

			return true;
		}

		public static async Task<bool> ValidateCommand(MyCommandContext context, int argPos)
		{
			//Admin check
			/*
			if (!(await context.Guild.GetCurrentUserAsync()).GuildPermissions.Administrator)
			{
				//If the server has been told already, ignore future commands fully
				if (!Variables.GuildsThatHaveBeenToldTheBotDoesNotWorkWithoutAdministratorAndWillBeIgnoredThuslyUntilTheyGiveTheBotAdministratorOrTheBotRestarts.Contains(context.Guild.Id))
				{
					await Actions.SendChannelMessage(context, "This bot will not function without the `Administrator` permission, sorry.");
					Variables.GuildsThatHaveBeenToldTheBotDoesNotWorkWithoutAdministratorAndWillBeIgnoredThuslyUntilTheyGiveTheBotAdministratorOrTheBotRestarts.Add(context.Guild.Id);
				}
				return false;
			}
			//Bot loaded check
			else*/ if (!Variables.Loaded)
			{
				await Actions.MakeAndDeleteSecondaryMessage(context, Actions.ERROR("Wait until the bot is loaded."));
				return false;
			}
			//Guild loaded check
			if (!((bool)context.GuildInfo.GetSetting(SettingOnGuild.Loaded)))
			{
				await Actions.MakeAndDeleteSecondaryMessage(context, Actions.ERROR("Wait until the guild is loaded."));
				return false;
			}
			//Ignored channel check
			else if (((List<ulong>)context.GuildInfo.GetSetting(SettingOnGuild.IgnoredCommandChannels)).Contains(context.Channel.Id))
			{
				return false;
			}
			//Command disabled check
			else if (!CheckIfCommandEnabled(context, argPos))
			{
				return false;
			}
			else
			{
				++Variables.AttemptedCommands;
				return true;
			}
		}
	}
}