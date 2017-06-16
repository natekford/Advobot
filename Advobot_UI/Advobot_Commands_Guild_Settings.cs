using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot
{
	//Guild Settings commands are commands that only affect that specific guild
	[Name("Guild_Settings")]
	public class Advobot_Commands_Guild_Settings : ModuleBase
	{
		[Command("leaveguild")]
		[Usage("<Guild ID>")]
		[Summary("Makes the bot leave the guild. Settings and preferences will be preserved.")]
		[OtherRequirement((1U << (int)Precondition.Guild_Owner) | (1U << (int)Precondition.Bot_Owner))]
		[DefaultEnabled(true)]
		public async Task GuildLeave([Optional, Remainder] string input)
		{
			//Get the guild out of an ID
			if (ulong.TryParse(input, out ulong guildID))
			{
				//Need bot owner check so only the bot owner can make the bot leave servers they don't own
				if (Context.User.Id == Variables.BotInfo.BotOwnerID)
				{
					var guild = Variables.Client.GetGuild(guildID);
					if (guild == null)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid server supplied."));
						return;
					}

					//Leave the server
					await guild.LeaveAsync();

					//Don't try to send a message if the guild left is the one the message was sent on
					if (Context.Guild == guild)
						return;

					await Actions.SendChannelMessage(Context, String.Format("Successfully left the server `{0}` with an ID `{1}`.", guild.Name, guild.Id));
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Only the bot owner can use this command targetting other guilds."));
					return;
				}
			}
			//No input means to leave the current guild
			else if (input == null)
			{
				await Actions.SendChannelMessage(Context, "Bye.");
				await Context.Guild.LeaveAsync();
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid server supplied."));
			}
		}

		[Command("modifyguildprefix")]
		[Alias("mgdp")]
		[Usage("[New Prefix|Clear]")]
		[Summary("Makes the guild use the given prefix from now on.")]
		[OtherRequirement(1U << (int)Precondition.Guild_Owner)]
		[DefaultEnabled(false)]
		public async Task GuildPrefix([Remainder] string input)
		{
			var guildInfo = await Actions.GetGuildInfo(Context.Guild);

			input = input.Trim().Replace("\n", "").Replace("\r", "");

			if (input.Length > 25)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Please do not try to make a prefix longer than 25 characters."));
				return;
			}
			else if (Actions.CaseInsEquals(input, Variables.BotInfo.Prefix))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("That prefix is already the global prefix."));
				return;
			}
			else if (Actions.CaseInsEquals(input, "clear"))
			{
				guildInfo.SetPrefix(null);
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully cleared the guild prefix.");
			}
			else
			{
				guildInfo.SetPrefix(input);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set this guild's prefix to: `{0}`.", input));
			}

			Actions.SaveGuildInfo(guildInfo);
		}

		[Command("displayguildsettings")]
		[Alias("dgds")]
		[Usage("<All|Setting Name> <Target:Channel|Role|User> <Extra:\"Additional Info\"")]
		[Summary("Displays guild settings. Inputting nothing gives a list of the setting names.")]
		[PermissionRequirement]
		[DefaultEnabled(true)]
		public async Task GuildSettings([Optional, Remainder] string input)
		{
			var guildInfo = await Actions.GetGuildInfo(Context.Guild);

			if (String.IsNullOrWhiteSpace(input))
			{
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Guild Settings", String.Format("`{0}`", String.Join("`, `", Enum.GetNames(typeof(SettingOnGuild))))));
				return;
			}

			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(0, 3), new[] { "target", "extra" });
			if (returnedArgs.Reason != ArgFailureReason.Not_Failure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var settingStr = returnedArgs.Arguments[0];
			var targetStr = returnedArgs.GetSpecifiedArg("target");
			var extraStr = returnedArgs.GetSpecifiedArg("extra");

			if (Actions.CaseInsEquals(settingStr, "all"))
			{
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Current Guild Settings", Actions.FormatAllSettings(guildInfo)));
			}
			else if (Enum.TryParse(settingStr, true, out SettingOnGuild setting))
			{
				var embed = await Actions.FormatSettingInfo(Context, guildInfo, setting, targetStr, extraStr);
				await Actions.SendEmbedMessage(Context.Channel, embed);
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid setting."));
			}
		}

		[Command("modifyguildfile")]
		[Alias("mgdf")]
		[Usage("[Reload|Resave|Reset]")]
		[Summary("Reload, resave, or reset the guild's settings on the bot. (Mainly for debug purposes when the JSON is edited manually)")]
		[OtherRequirement(1U << (int)Precondition.Guild_Owner)]
		[DefaultEnabled(true)]
		public async Task GuildReload([Remainder] string input)
		{
			var guildInfo = await Actions.GetGuildInfo(Context.Guild);

			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 1));
			if (returnedArgs.Reason != ArgFailureReason.Not_Failure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];

			var guild = Context.Guild;
			if (Actions.CaseInsEquals(actionStr, "reload"))
			{
				Variables.Guilds.Remove(guild.Id);
				Variables.Guilds.Add(guild.Id, await Actions.CreateGuildInfo(guild));

				await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully reloaded the guild's bot information.");
			}
			else if (Actions.CaseInsEquals(actionStr, "resave"))
			{
				Actions.SaveGuildInfo(guildInfo);
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully resaved the guild's bot information.");
			}
			else if (Actions.CaseInsEquals(actionStr, "reset"))
			{
				Variables.Guilds.Remove(guild.Id);

				var path = Actions.GetServerFilePath(guild.Id, Constants.GUILD_INFO_LOCATION);
				File.Delete(path);

				Variables.Guilds.Add(guild.Id, await Actions.CreateGuildInfo(guild));
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully reset the guild's bot information.");
			}
		}

		[Command("configurecommands")]
		[Alias("concom", "cncm")]
		[Usage("[Enable|Disable] [Command Name|Category Name|All]")]
		[Summary("Turns a command on or off. Can turn all commands in a category on or off too. Cannot turn off `configurecommands` or `help`.")]
		[PermissionRequirement]
		[DefaultEnabled(true)]
		public async Task CommandConfig([Remainder] string input)
		{
			var guildInfo = await Actions.GetGuildInfo(Context.Guild);

			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 2));
			if (returnedArgs.Reason != ArgFailureReason.Not_Failure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];
			var cmdStr = returnedArgs.Arguments[1];

			var returnedType = Actions.GetType(actionStr, new[] { ActionType.Enable, ActionType.Disable });
			if (returnedType.Reason != TypeFailureReason.Not_Failure)
			{
				await Actions.HandleTypeGettingErrors(Context, returnedType);
				return;
			}
			var action = returnedType.Type;

			//Check if all
			var allBool = false;
			if (Actions.CaseInsEquals(cmdStr, "all"))
			{
				allBool = true;
			}

			var command = Actions.GetCommand(guildInfo, cmdStr);
			var commands = new List<CommandSwitch>();
			if (allBool)
			{
				commands = guildInfo.CommandOverrides.Commands;
			}
			else if (command == null)
			{
				if (Enum.TryParse(cmdStr, true, out CommandCategory cmdCat))
				{
					commands = Actions.GetMultipleCommands(guildInfo, cmdCat);
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No command or category has that name."));
					return;
				}
			}

			switch (action)
			{
				case ActionType.Enable:
				{
					if (command != null && command.ValAsBoolean)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This command is already enabled."));
						return;
					}
					break;
				}
				case ActionType.Disable:
				{
					if (command != null && !command.ValAsBoolean)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This command is already disabled."));
						return;
					}
					break;
				}
			}

			//Add the command to the category list for simpler usage later
			if (command != null)
			{
				commands.Add(command);
			}
			//Find the commands that shouldn't be turned off
			var unableToBeRemoved = new List<CommandSwitch>();
			commands.ForEach(cmd =>
			{
				if (Constants.COMMANDS_UNABLE_TO_BE_TURNED_OFF.CaseInsContains(cmd.Name))
				{
					unableToBeRemoved.Add(cmd);
				}
			});
			commands = commands.Except(unableToBeRemoved).ToList();

			if (commands.Count < 1)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Please don't try to edit that command."));
				return;
			}

			var pastTense = "";
			var presentTense = "";
			switch (action)
			{
				case ActionType.Enable:
				{
					commands.ForEach(x => x.Enable());
					pastTense = "enabled";
					presentTense = "enable";
					break;
				}
				case ActionType.Disable:
				{
					commands.ForEach(x => x.Disable());
					pastTense = "disabled";
					presentTense = "disable";
					break;
				}
			}

			//Save the preferences
			Actions.SaveGuildInfo(guildInfo);
			var desc = Actions.FormatResponseMessagesForCmdsOnLotsOfObjects(commands.Select(x => x.Name), unableToBeRemoved.Select(x => x.Name), "command", pastTense, presentTense);
			await Actions.SendChannelMessage(Context, desc);
		}

		[Command("modifyignoredcommandchannels")]
		[Alias("micc")]
		[Usage("[Add|Remove] [Channel] <Command Name|Category Name>")]
		[Summary("The bot will ignore commands said on these channels. If a command is input then the bot will instead ignore only that command on the given channel.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task CommandIgnore([Remainder] string input)
		{
			var guildInfo = await Actions.GetGuildInfo(Context.Guild);

			//Split the input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 3));
			if (returnedArgs.Reason != ArgFailureReason.Not_Failure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];
			var chanStr = returnedArgs.Arguments[1];
			var cmdStr = returnedArgs.Arguments[2];

			//Get the channel
			var returnedChannel = Actions.GetChannel(Context, new[] { ChannelCheck.Can_Modify_Permissions }, true, chanStr);
			if (returnedChannel.Reason != FailureReason.Not_Failure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedChannel);
				return;
			}
			var channel = returnedChannel.Object;

			var returnedType = Actions.GetType(input, new[] { ActionType.Add, ActionType.Remove });
			if (returnedType.Reason != TypeFailureReason.Not_Failure)
			{
				await Actions.HandleTypeGettingErrors(Context, returnedType);
				return;
			}
			var action = returnedType.Type;
			var add = action == ActionType.Add;

			//Get the lists the bot will use for this command
			var ignoredCmdChannels = guildInfo.IgnoredCommandChannels;
			var ignoredCmdsOnChans = guildInfo.CommandOverrides.Channels;
			if (!String.IsNullOrWhiteSpace(cmdStr))
			{
				var cmd = Variables.CommandNames.FirstOrDefault(x => Actions.CaseInsEquals(x, cmdStr));
				var catCmds = Enum.TryParse(cmdStr, true, out CommandCategory cat) ? Variables.HelpList.Where(x => x.Category == cat).ToList() : null;
				if (cmd == null && catCmds == null)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("The given input `{0}` is not a valid command or category.", cmdStr));
					return;
				}

				if (cmd != null)
				{
					if (add)
					{
						if (ignoredCmdsOnChans.Any(x => x.Name == cmd && x.ID == channel.Id))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This command is already ignored on this channel."));
							return;
						}
						ignoredCmdsOnChans.Add(new CommandOverride<IGuildChannel>(cmd, channel.Id, false));
					}
					else
					{
						var amtRemoved = ignoredCmdsOnChans.RemoveAll(x => x.Name == cmd && x.ID == channel.Id);
						if (amtRemoved == 0)
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This command is already not ignored on this channel."));
							return;
						}
					}
					await Actions.MakeAndDeleteSecondaryMessage(Context,
						String.Format("Successfully {0} the command `{1}` in `{2}`.", add ? "disabled" : "enabled", cmd, channel.FormatChannel()));
				}
				else if (catCmds != null)
				{
					if (add)
					{
						catCmds.ForEach(x =>
						{
							if (!ignoredCmdsOnChans.Any(y => y.Name == x.Name && y.ID == channel.Id))
							{
								ignoredCmdsOnChans.Add(new CommandOverride<IGuildChannel>(x.Name, channel.Id, false));
							}
						});
					}
					else
					{
						catCmds.ForEach(x =>
						{
							ignoredCmdsOnChans.RemoveAll(y => y.Name == cmd && y.ID == channel.Id);
						});
					}
					await Actions.MakeAndDeleteSecondaryMessage(Context,
						String.Format("Successfully {0} the category `{1}` in `{2}`.", add ? "disabled" : "enabled", Enum.GetName(typeof(CommandCategory), cat), channel.FormatChannel()));
				}

				//Save everything and send a success message
				Actions.SaveGuildInfo(guildInfo);
			}
			else
			{
				//Add or remove
				if (add)
				{
					if (ignoredCmdChannels.Contains(channel.Id))
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This channel is already ignored for commands."));
						return;
					}
					ignoredCmdChannels.Add(channel.Id);
				}
				else
				{
					if (!ignoredCmdChannels.Contains(channel.Id))
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This channel is already not ignored for commands."));
						return;
					}
					ignoredCmdChannels.Remove(channel.Id);
				}
				ignoredCmdChannels = ignoredCmdChannels.Distinct().ToList();

				var outputStr = "";
				if (add)
				{
					outputStr = String.Format("Successfully added the channel `{0}` to the command ignore list.", channel.FormatChannel());
				}
				else
				{
					outputStr = String.Format("Successfully removed the channel `{0}` from the command ignore list.", channel.FormatChannel());
				}

				//Save everything and send a success message
				Actions.SaveGuildInfo(guildInfo);
				await Actions.MakeAndDeleteSecondaryMessage(Context, outputStr);
			}
		}

		[Command("modifybotusers")]
		[Alias("mbu")]
		[Usage("[Show|Add|Remove] [User] [Permission/...]")]
		[Summary("Gives a user permissions in the bot but not on Discord itself. Type `" + Constants.BOT_PREFIX + "mbu [Show]` to see the available permissions. " +
			"Type `" + Constants.BOT_PREFIX + "mbu [Show] [User]` to see the permissions of that user.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task BotUsersModify([Optional, Remainder] string input)
		{
			var guildInfo = await Actions.GetGuildInfo(Context.Guild);

			//Split input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 3));
			if (returnedArgs.Reason != ArgFailureReason.Not_Failure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];
			var userStr = returnedArgs.Arguments[1];
			var permStr = returnedArgs.Arguments[2];

			//Check if valid action
			var returnedType = Actions.GetType(actionStr, new[] { ActionType.Show, ActionType.Add, ActionType.Remove });
			if (returnedType.Reason != TypeFailureReason.Not_Failure)
			{
				await Actions.HandleTypeGettingErrors(Context, returnedType);
				return;
			}
			var action = returnedType.Type;

			if (returnedArgs.ArgCount == 1)
			{
				if (action == ActionType.Show)
				{
					await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Guild Permissions", String.Format("`{0}`", String.Join("`, `", Variables.GuildPermissions.Select(x => x.Name)))));
					return;
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
					return;
				}
			}

			//Get the user
			var returnedUser = Actions.GetGuildUser(Context, new[] { UserCheck.Can_Be_Edited }, true, userStr);
			if (returnedUser.Reason != FailureReason.Not_Failure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedUser);
				return;
			}
			var user = returnedUser.Object;

			//Get the botuser
			var botUser = guildInfo.BotUsers.FirstOrDefault(x => x.User == user);
			switch (action)
			{
				case ActionType.Show:
				{
					if (returnedArgs.ArgCount == 2)
					{
						if (botUser == null || botUser.Permissions == 0)
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("That user has no extra permissions from the bot."));
							return;
						}

						//check if that user has any permissions
						var showPerms = Actions.GetPermissionNames(botUser.Permissions);
						if (!showPerms.Any())
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("That user has no extra permissions from the bot."));
						}
						else
						{
							var title = String.Format("Permissions for {0}", user.FormatUser());
							var desc = String.Format("`{0}`", String.Join("`, `", showPerms));
							await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(title, desc));
						}
					}
					else
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid option for show."));
					}
					return;
				}
				case ActionType.Remove:
				{
					if (returnedArgs.ArgCount == 2)
					{
						if (botUser == null)
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("That user is not on the bot user list."));
							return;
						}

						guildInfo.BotUsers.Remove(botUser);
						Actions.SaveGuildInfo(guildInfo);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed `{0}` from the bot user list.", user.FormatUser()));
						return;
					}
					break;
				}
			}

			//Get the permissions
			var permissions = permStr.Split('/').Select(x => Variables.GuildPermissions.FirstOrDefault(y => Actions.CaseInsEquals(y.Name, x))).Where(x => x.Name != null).ToList();
			if (!permissions.Any())
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid input for permissions."));
				return;
			}
			else if (Context.User.Id != Context.Guild.OwnerId)
			{
				permissions.RemoveAll(x => Actions.CaseInsEquals(x.Name, Enum.GetName(typeof(GuildPermission), GuildPermission.Administrator)));
			}

			//Modify the user's perms
			botUser = botUser ?? new BotImplementedPermissions(Context.Guild.Id, user.Id, 0, guildInfo);
			var outputStr = "";
			switch (action)
			{
				case ActionType.Add:
				{
					permissions.ForEach(x =>
					{
						botUser.AddPermission(x.Position);
					});
					outputStr = String.Format("gave the user `{0}` the following bot permission{1}", user.FormatUser(), Actions.GetPlural(permissions.Count));
					break;
				}
				case ActionType.Remove:
				{
					permissions.ForEach(x =>
					{
						if (botUser.Permissions == 0)
							return;
						botUser.RemovePermission(x.Position);
					});
					outputStr = String.Format("removed the following bot permission{0} from the user `{1}`", Actions.GetPlural(permissions.Count), user.FormatUser());
					break;
				}
			}
			if (botUser.Permissions == 0)
			{
				guildInfo.BotUsers.ThreadSafeRemoveAll(x => x.UserID == botUser.UserID);
			}

			//Save everything and send a success message
			Actions.SaveGuildInfo(guildInfo);
			await Actions.SendChannelMessage(Context, String.Format("Successfully {0}: `{1}`.", outputStr, String.Join("`, `", permissions.Select(x => x.Name))));
		}

		[Command("modifychannelsettings")]
		[Alias("mcs")]
		[Usage("[ImageOnly|Sanitary] <Channel>")]
		[Summary("Image only works solely on attachments. Sanitary means any message sent by someone without admin gets deleted. " +
			"No input channel means it applies to the current channel. Using the command on an already targetted channel turned it off.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageChannels)]
		[DefaultEnabled(false)]
		public async Task ModifyImageOnly([Optional, Remainder] string input)
		{
			var guildInfo = await Actions.GetGuildInfo(Context.Guild);

			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 2));
			if (returnedArgs.Reason != ArgFailureReason.Not_Failure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var settingStr = returnedArgs.Arguments[0];
			var chanStr = returnedArgs.Arguments[1];

			var returnedType = Actions.GetType(settingStr, new[] { ChannelSettings.ImageOnly, ChannelSettings.Sanitary });
			if (returnedType.Reason != TypeFailureReason.Not_Failure)
			{
				await Actions.HandleTypeGettingErrors(Context, returnedType);
				return;
			}
			var type = returnedType.Type;

			var channel = Actions.GetChannel(Context, new[] { ChannelCheck.Can_Be_Managed, ChannelCheck.Can_Delete_Messages, ChannelCheck.Is_Text }, true, input).Object ?? Context.Channel as ITextChannel;
			switch (type)
			{
				case ChannelSettings.ImageOnly:
				{
					if (guildInfo.ImageOnlyChannels.Contains(channel.Id))
					{
						guildInfo.ImageOnlyChannels.Remove(channel.Id);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed the channel `{0}` from the image only list."));
					}
					else
					{
						guildInfo.ImageOnlyChannels.Add(channel.Id);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully added the channel `{0}` to the image only list."));
					}
					break;
				}
				case ChannelSettings.Sanitary:
				{
					if (guildInfo.SanitaryChannels.Contains(channel.Id))
					{
						guildInfo.SanitaryChannels.Remove(channel.Id);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed the channel `{0}` from the sanitary list."));
					}
					else
					{
						guildInfo.SanitaryChannels.Add(channel.Id);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully added the channel `{0}` to the sanitary list."));
					}
					break;
				}
			}
		}

		[Command("modifyreminds")]
		[Alias("mrem")]
		[Usage("[Add|Remove] [\"Name\"] <\"Text\">")]
		[Summary("Adds the given text to a list that can be called through the `remind` command.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task RemindsModify([Remainder] string input)
		{
			var guildInfo = await Actions.GetGuildInfo(Context.Guild);

			//Split the input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 3));
			if (returnedArgs.Reason != ArgFailureReason.Not_Failure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];
			var nameStr = returnedArgs.Arguments[1];
			var textStr = returnedArgs.Arguments[2];

			var returnedType = Actions.GetType(actionStr, new[] { ActionType.Add, ActionType.Remove });
			if (returnedType.Reason != TypeFailureReason.Not_Failure)
			{
				await Actions.HandleTypeGettingErrors(Context, returnedType);
				return;
			}
			var action = returnedType.Type;
			var add = action == ActionType.Add;

			var reminds = guildInfo.Reminds;
			nameStr = Actions.ReplaceMarkdownChars(nameStr);
			if (add)
			{
				//Check if at the max number of reminds
				if (reminds.Count >= Constants.MAX_REMINDS)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild already has the max number of reminds, which is 50."));
					return;
				}

				//Check if any reminds have already have the same name
				if (reminds.Any(x => Actions.CaseInsEquals(x.Name, nameStr)))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("A remind already has that name."));
					return;
				}

				//Make sure there's text
				if (String.IsNullOrWhiteSpace(textStr))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Adding a remind requires text."));
					return;
				}

				//Add them to the list
				guildInfo.Reminds.Add(new Remind(nameStr, textStr));
			}
			else
			{
				//Make sure there are some reminds
				if (!reminds.Any())
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("There needs to be at least one remind before you can remove any."));
					return;
				}

				//Remove all reminds with the same name
				reminds.RemoveAll(x => Actions.CaseInsEquals(x.Name, nameStr));
			}

			//Save everything and send a success message
			Actions.SaveGuildInfo(guildInfo);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} the following remind: `{1}`.", add ? "added" : "removed", nameStr));
		}

		[Command("sayremind")]
		[Alias("sr")]
		[Usage("<Name>")]
		[Summary("Shows the content for the given remind. If null then shows the list of the current reminds.")]
		[OtherRequirement(1U << (int)Precondition.User_Has_A_Perm)]
		[DefaultEnabled(false)]
		public async Task Reminds([Optional, Remainder] string input)
		{
			var guildInfo = await Actions.GetGuildInfo(Context.Guild);
			var reminds = guildInfo.Reminds;
			if (String.IsNullOrWhiteSpace(input))
			{
				//Check if any exist
				if (!reminds.Any())
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("There are no reminds."));
				}
				else
				{
					//Send the names of all of the reminds
					await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Reminds", String.Format("`{0}`", String.Join("`, `", reminds.Select(x => x.Name)))));
				}
				return;
			}

			//Check if any reminds have the given name
			var remind = reminds.FirstOrDefault(x => Actions.CaseInsEquals(x.Name, input));
			if (remind.Name != null)
			{
				await Actions.SendChannelMessage(Context, remind.Text);
			}
			else
			{
				//Find close words
				var closeWords = Actions.GetRemindsWithSimilarNames(reminds, input).Distinct().ToList();

				if (closeWords.Any())
				{
					//Format a message to be said
					var count = 1;
					var msg = "Did you mean any of the following:\n" + String.Join("\n", closeWords.Select(x => String.Format("`{0}.` {1}", count++.ToString("00"), x.Name)));

					//Create the list, add it to the guild, remove it after five seconds, and delete the message that goes along with it after 5 seconds
					var acWords = new ActiveCloseWords(Context.User as IGuildUser, closeWords);
					lock (Variables.ActiveCloseWords)
					{
						Variables.ActiveCloseWords.RemoveAll(x => x.User == Context.User);
						Variables.ActiveCloseWords.Add(acWords);
					}
					await Actions.MakeAndDeleteSecondaryMessage(Context, msg, 5000);
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Nothing similar to that remind can be found."));
				}
			}
		}

		[Command("setguildnotif")]
		[Alias("sgn")]
		[Usage("[Welcome|Goodbye] [#Channel] <\"Content:string\"> <\"Title:string\"> <\"Desc:string\"> <\"Thumb:string\">")]
		[Summary("The bot send a message to the given channel when the self explantory event happens. `{User}` will be replaced with the formatted user.  `{UserMention}` will be replaced with a mention of the joining user.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task SetGuildNotif([Remainder] string input)
		{
			var guildInfo = await Actions.GetGuildInfo(Context.Guild);

			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(3, 6), new[] { "content", "title", "desc", "thumb" });
			if (returnedArgs.Reason != ArgFailureReason.Not_Failure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var typeStr = returnedArgs.Arguments[0];
			var chanStr = returnedArgs.Arguments[1];
			var contStr = returnedArgs.GetSpecifiedArg("content");
			var titleStr = returnedArgs.GetSpecifiedArg("title");
			var descStr = returnedArgs.GetSpecifiedArg("desc");
			var thumbStr = returnedArgs.GetSpecifiedArg("thumb");
			thumbStr = Actions.ValidateURL(thumbStr) ? thumbStr : null;

			//Check if everything is null
			var contentB = String.IsNullOrWhiteSpace(contStr);
			var titleB = String.IsNullOrWhiteSpace(titleStr);
			var descB = String.IsNullOrWhiteSpace(descStr);
			var thumbB = String.IsNullOrWhiteSpace(thumbStr);
			if (contentB && titleB && descB && thumbB)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("One of the variables has to be given."));
				return;
			}

			//Make sure the target type is valid
			var returnedType = Actions.GetType(typeStr, new[] { GuildNotifications.Welcome, GuildNotifications.Goodbye });
			if (returnedType.Reason != TypeFailureReason.Not_Failure)
			{
				await Actions.HandleTypeGettingErrors(Context, returnedType);
				return;
			}
			var type = returnedType.Type;

			//Make sure the channel mention is valid
			var returnedChannel = Actions.GetChannel(Context, new[] { ChannelCheck.Can_Modify_Permissions, ChannelCheck.Is_Text }, true, chanStr);
			if (returnedChannel.Reason != FailureReason.Not_Failure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedChannel);
				return;
			}
			var channel = returnedChannel.Object as ITextChannel;

			var guildNotif = new GuildNotification(contStr, titleStr, descStr, thumbStr, Context.Guild.Id, channel.Id);
			switch (type)
			{
				case GuildNotifications.Welcome:
				{
					guildInfo.SetWelcomeMessage(guildNotif);
					await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully set the welcome message.");
					break;
				}
				case GuildNotifications.Goodbye:
				{
					guildInfo.SetGoodbyeMessage(guildNotif);
					await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully set the goodbye message.");
					break;
				}
			}

			Actions.SaveGuildInfo(guildInfo);
		}

		[Command("testguildnotif")]
		[Alias("tgn")]
		[Usage("[Welcome|Goodbye]")]
		[Summary("Sends the given guild notification in order to test it.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task TestGuildNotification([Remainder] string input)
		{
			var guildInfo = await Actions.GetGuildInfo(Context.Guild);

			if (!Enum.TryParse(input, true, out GuildNotifications notifType))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid notification type supplied."));
				return;
			}

			GuildNotification notif = null;
			switch (notifType)
			{
				case GuildNotifications.Welcome:
				{
					notif = guildInfo.WelcomeMessage;
					break;
				}
				case GuildNotifications.Goodbye:
				{
					notif = guildInfo.GoodbyeMessage;
					break;
				}
			}

			if (notif == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The notification does not exist."));
				return;
			}

			await Actions.SendGuildNotification(null, notif);
		}

		[Command("getfile")]
		[Alias("gf")]
		[Usage("")]
		[Summary("Sends the file containing all the guild's saved bot information.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task GetFile()
		{
			var path = Actions.GetServerFilePath(Context.Guild.Id, Constants.GUILD_INFO_LOCATION);
			if (!File.Exists(path))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The guild information file does not exist at this time."));
				return;
			}
			await Actions.UploadFile(Context.Channel, path);
		}
	}
}
