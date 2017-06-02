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
			var channel = message.Channel as Discord.ITextChannel;
			if (channel == null)
				return;

			if (!Variables.Guilds.TryGetValue(channel.GuildId, out BotGuildInfo guildInfo))
			{
				await Actions.LoadGuild(channel.Guild);
				guildInfo = Variables.Guilds[channel.GuildId];
			}

			if (!guildInfo.Loaded)
			{
				await Actions.SendChannelMessage(channel, "The guild is not fully loaded, please wait.");
				return;
			}
			else if (guildInfo.IgnoredCommandChannels.Contains(channel.Id))
				return;

			var argPos = await PrefixHandling(message, guildInfo.Prefix);
			if (argPos == -1)
				return;

			//Check if there is anything preventing the command from going through
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

		public static async Task<int> PrefixHandling(SocketUserMessage message, string guildPrefix)
		{
			var argPos = 0;
			if (String.IsNullOrWhiteSpace(guildPrefix))
			{
				if (message.HasStringPrefix(Variables.BotInfo.Prefix, ref argPos))
				{
					return argPos;
				}
			}
			else
			{
				if (message.HasStringPrefix(guildPrefix, ref argPos))
				{
					return argPos;
				}
				else if (message.HasMentionPrefix(Variables.Client.GetCurrentUser(), ref argPos))
				{
					await Actions.SendChannelMessage(message.Channel, String.Format("The guild's current prefix is: `{0}`.", guildPrefix));
				}
			}
			return -1;
		}

		public static async Task<bool> ValidateCommand(BotGuildInfo guildInfo, ICommandContext context, int argPos)
		{
			//Check to make sure everything is loaded
			if (!Variables.Loaded)
			{
				await Actions.MakeAndDeleteSecondaryMessage(context, Actions.ERROR("Please wait until everything the bot is loaded."));
				return false;
			}
			//Check if a command is disabled
			else if (!CheckCommandEnabled(guildInfo, context, argPos))
			{
				return false;
			}
			//Check if the bot still has admin
			else if (!(await context.Guild.GetCurrentUserAsync()).GuildPermissions.Administrator)
			{
				//If the server has been told already, ignore future commands fully
				if (Variables.GuildsThatHaveBeenToldTheBotDoesNotWorkWithoutAdministratorAndWillBeIgnoredThuslyUntilTheyGiveTheBotAdministratorOrTheBotRestarts.Contains(context.Guild))
					return false;

				await Actions.SendChannelMessage(context, "This bot will not function without the `Administrator` permission, sorry.");
				Variables.GuildsThatHaveBeenToldTheBotDoesNotWorkWithoutAdministratorAndWillBeIgnoredThuslyUntilTheyGiveTheBotAdministratorOrTheBotRestarts.Add(context.Guild);
				return false;
			}
			else
			{
				++Variables.AttemptedCommands;
				return true;
			}
		}

		public static bool CheckCommandEnabled(BotGuildInfo guildInfo, ICommandContext context, int argPos)
		{
			if (context.Guild == null)
				return false;

			//Get the command
			var cmdName = context.Message.Content.Substring(argPos).Split(' ').FirstOrDefault();
			var cmd = Actions.GetCommand(guildInfo, cmdName);
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
			 * Else if role is set, use role setting
			 * Else if channel is set, use channel setting
			 * Else default to current command switch setting
			 */

			var user = guildInfo.CommandOverrides.Users.FirstOrDefault(x => x.ID == context.User.Id && Actions.CaseInsEquals(cmd.Name, x.Name));
			if (user != null)
			{
				return user.Enabled;
			}

			var roles = guildInfo.CommandOverrides.Roles.Where(x => (context.User as Discord.IGuildUser).RoleIds.Contains(x.ID) && Actions.CaseInsEquals(cmd.Name, x.Name)).ToList();
			if (roles.Any())
			{
				return !roles.Any(x => !x.Enabled);
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