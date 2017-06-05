using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using System.Threading.Tasks;
using System;
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
			//Create Command Service, inject it into Dependency Map
			Client = (BotClient)provider.GetService(typeof(BotClient));
			Commands = new CommandService();
			Provider = provider;

			await Commands.AddModulesAsync(Assembly.GetEntryAssembly());
		}

		public static async Task HandleCommand(SocketMessage parameterMessage)
		{
			if (Variables.Pause)
				return;
			var message = parameterMessage as SocketUserMessage;
			if (message == null)
				return;
			var guild = (message.Channel as SocketTextChannel)?.Guild;
			if (guild == null)
				return;

			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
			{
				await Actions.LoadGuild(guild);
				guildInfo = Variables.Guilds[guild.Id];
			}

			if (!PrefixHandling(message, guildInfo.Prefix, out int argPos))
				return;

			var context = new CommandContext(Client.GetClient(), message);
			if (!await ValidateCommand(guildInfo, context, argPos))
				return;

			//Ignore unknown command errors because they're annoying and ignore the errors given by lack of permissions, etc. put in by me
			var result = await Commands.ExecuteAsync(context, argPos, Provider);
			if (result.IsSuccess)
			{
				await Mod_Logs.LogCommand(guildInfo, context);
			}
			else
			{
				if (!(Actions.CaseInsEquals(result.ErrorReason, Constants.IGNORE_ERROR) || result.Error == CommandError.UnknownCommand))
				{
					await Actions.MakeAndDeleteSecondaryMessage(context, Actions.ERROR(result.ErrorReason));
				}
			}
		}

		public static bool PrefixHandling(SocketUserMessage message, string guildPrefix, out int argPos)
		{
			argPos = -1;
			if (String.IsNullOrWhiteSpace(guildPrefix))
			{
				if (message.HasStringPrefix(Variables.BotInfo.Prefix, ref argPos))
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

		public static async Task<bool> ValidateCommand(BotGuildInfo guildInfo, ICommandContext context, int argPos)
		{
			//Admin check
			if (!(await context.Guild.GetCurrentUserAsync()).GuildPermissions.Administrator)
			{
				//If the server has been told already, ignore future commands fully
				if (Variables.GuildsThatHaveBeenToldTheBotDoesNotWorkWithoutAdministratorAndWillBeIgnoredThuslyUntilTheyGiveTheBotAdministratorOrTheBotRestarts.Contains(context.Guild))
					return false;

				await Actions.SendChannelMessage(context, "This bot will not function without the `Administrator` permission, sorry.");
				Variables.GuildsThatHaveBeenToldTheBotDoesNotWorkWithoutAdministratorAndWillBeIgnoredThuslyUntilTheyGiveTheBotAdministratorOrTheBotRestarts.Add(context.Guild);
				return false;
			}
			//Bot loaded check
			else if (!Variables.Loaded)
			{
				await Actions.MakeAndDeleteSecondaryMessage(context, Actions.ERROR("Wait until the bot is loaded."));
				return false;
			}
			//Guild loaded check
			if (!guildInfo.Loaded)
			{
				await Actions.MakeAndDeleteSecondaryMessage(context, Actions.ERROR("Wait until the guild is loaded."));
				return false;
			}
			//Ignored channel check
			else if (guildInfo.IgnoredCommandChannels.Contains(context.Channel.Id))
			{
				return false;
			}
			//Command disabled check
			else if (!CheckIfCommandEnabled(guildInfo, context, argPos))
			{
				return false;
			}
			else
			{
				++Variables.AttemptedCommands;
				return true;
			}
		}

		public static bool CheckIfCommandEnabled(BotGuildInfo guildInfo, ICommandContext context, int argPos)
		{
			if (context.Guild == null)
				return false;

			//Get the command
			var cmd = Actions.GetCommand(guildInfo, context.Message.Content.Substring(argPos).Split(' ').FirstOrDefault());
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

			var user = guildInfo.CommandOverrides.Users.FirstOrDefault(x => x.ID == context.User.Id && Actions.CaseInsEquals(cmd.Name, x.Name));
			if (user != null)
			{
				return user.Enabled;
			}

			var role = guildInfo.CommandOverrides.Roles.Where(x => context.Guild.Roles.Select(y => y.Id).Contains(x.ID) && Actions.CaseInsEquals(cmd.Name, x.Name)).OrderBy(x => context.Guild.GetRole(x.ID).Position).LastOrDefault();
			if (role != null)
			{
				return role.Enabled;
			}

			var channel = guildInfo.CommandOverrides.Channels.FirstOrDefault(x => x.ID == context.Channel.Id && Actions.CaseInsEquals(cmd.Name, x.Name));
			if (channel != null)
			{
				return channel.Enabled;
			}

			return true;
		}
	}
}