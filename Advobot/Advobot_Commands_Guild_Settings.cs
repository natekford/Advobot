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
	[Name("GuildSettings")]
	public class Advobot_Commands_Guild_Settings : ModuleBase
	{
		[Command("leaveguild")]
		[Usage("<Guild ID>")]
		[Summary("Makes the bot leave the guild. Settings and preferences will be preserved.")]
		[OtherRequirement(Precondition.GuildOwner | Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public async Task GuildLeave([Optional, Remainder] string input)
		{
			//Get the guild out of an ID
			if (ulong.TryParse(input, out ulong guildID))
			{
				//Need bot owner check so only the bot owner can make the bot leave servers they don't own
				if (Context.User.Id == ((ulong)Variables.BotInfo.GetSetting(SettingOnBot.BotOwnerID)))
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
				else if (Context.Guild.Id == guildID)
				{
					await Context.Guild.LeaveAsync();
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
		[OtherRequirement(Precondition.GuildOwner)]
		[DefaultEnabled(false)]
		public async Task GuildPrefix([Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);
			var globalPrefix = ((string)Variables.BotInfo.GetSetting(SettingOnBot.Prefix));

			input = input.Trim().Replace("\n", "").Replace("\r", "");

			if (input.Length > 25)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Please do not try to make a prefix longer than 25 characters."));
				return;
			}
			else if (Actions.CaseInsEquals(input, globalPrefix))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("That prefix is already the global prefix."));
				return;
			}
			else if (Actions.CaseInsEquals(input, "clear"))
			{
				if (guildInfo.SetSetting(SettingOnGuild.Prefix, null))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully cleared the guild's prefix.");
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Failed to clear the guild's prefix."));
				}
			}
			else
			{
				if (guildInfo.SetSetting(SettingOnGuild.Prefix, input))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set this guild's prefix to: `{0}`.", input));
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Failed to set the guild's prefix."));
				}
			}
		}

		[Command("displayguildsettings")]
		[Alias("dgds")]
		[Usage("<All|Setting Name>")]
		[Summary("Displays guild settings. Inputting nothing gives a list of the setting names.")]
		[PermissionRequirement]
		[DefaultEnabled(true)]
		public async Task GuildSettings([Optional, Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);
			if (String.IsNullOrWhiteSpace(input))
			{
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Guild Settings", String.Format("`{0}`", String.Join("`, `", Enum.GetNames(typeof(SettingOnGuild))))));
				return;
			}

			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(0, 1));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var settingStr = returnedArgs.Arguments[0];

			var guild = Context.Guild as SocketGuild;
			if (Actions.CaseInsEquals(settingStr, "all"))
			{
				await Actions.WriteAndUploadTextFile(Context.Guild, Context.Channel, Actions.FormatAllSettings(guild, guildInfo), "Current_Guild_Settings");
			}
			else if (Enum.TryParse(settingStr, true, out SettingOnGuild setting))
			{
				var title = setting.EnumName();
				var desc = Actions.FormatSettingInfo(guild, guildInfo, setting);
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(title, desc));
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
		[OtherRequirement(Precondition.GuildOwner)]
		[DefaultEnabled(true)]
		public async Task GuildReload([Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 1));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];

			var guild = Context.Guild;
			if (Actions.CaseInsEquals(actionStr, "reload"))
			{
				Variables.Guilds.Remove(guild.Id);
				await Actions.CreateOrGetGuildInfo(guild);

				await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully reloaded the guild's bot information.");
			}
			else if (Actions.CaseInsEquals(actionStr, "resave"))
			{
				guildInfo.SaveInfo();
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully resaved the guild's bot information.");
			}
			else if (Actions.CaseInsEquals(actionStr, "reset"))
			{
				Variables.Guilds.Remove(guild.Id);

				var path = Actions.GetServerFilePath(guild.Id, Constants.GUILD_INFO_LOCATION);
				File.Delete(path);

				await Actions.CreateOrGetGuildInfo(guild);
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
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 2));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];
			var cmdStr = returnedArgs.Arguments[1];

			var returnedType = Actions.GetEnum(actionStr, new[] { ActionType.Enable, ActionType.Disable });
			if (returnedType.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedType);
				return;
			}
			var action = returnedType.Object;

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
				commands = ((List<CommandSwitch>)guildInfo.GetSetting(SettingOnGuild.CommandSwitches));
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

			guildInfo.SaveInfo();
			await Actions.SendChannelMessage(Context, Actions.FormatResponseMessagesForCmdsOnLotsOfObjects(commands.Select(x => x.Name), unableToBeRemoved.Select(x => x.Name), "command", pastTense, presentTense));
		}

		[Command("modifyignoredcommandchannels")]
		[Alias("micc")]
		[Usage("[Add|Remove] [Channel] <Command Name|Category Name>")]
		[Summary("The bot will ignore commands said on these channels. If a command is input then the bot will instead ignore only that command on the given channel.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task CommandIgnore([Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			//Split the input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 3));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];
			var chanStr = returnedArgs.Arguments[1];
			var cmdStr = returnedArgs.Arguments[2];

			//Get the channel
			var returnedChannel = Actions.GetChannel(Context, new[] { ObjectVerification.CanModifyPermissions }, true, chanStr);
			if (returnedChannel.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedChannel);
				return;
			}
			var channel = returnedChannel.Object;

			var returnedType = Actions.GetEnum(actionStr, new[] { ActionType.Add, ActionType.Remove });
			if (returnedType.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedType);
				return;
			}
			var action = returnedType.Object;
			var add = action == ActionType.Add;

			//Get the lists the bot will use for this command
			var ignoredCmdChannels = ((List<ulong>)guildInfo.GetSetting(SettingOnGuild.IgnoredCommandChannels));
			var ignoredCmdsOnChans = ((List<CommandOverride>)guildInfo.GetSetting(SettingOnGuild.CommandsDisabledOnChannel));
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
						ignoredCmdsOnChans.Add(new CommandOverride(cmd, channel.Id, false));
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
								ignoredCmdsOnChans.Add(new CommandOverride(x.Name, channel.Id, false));
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
						String.Format("Successfully {0} the category `{1}` in `{2}`.", add ? "disabled" : "enabled", cat.EnumName(), channel.FormatChannel()));
				}

				guildInfo.SaveInfo();
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

				guildInfo.SaveInfo();
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
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			//Split input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 3));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];
			var userStr = returnedArgs.Arguments[1];
			var permStr = returnedArgs.Arguments[2];

			//Check if valid action
			var returnedType = Actions.GetEnum(actionStr, new[] { ActionType.Show, ActionType.Add, ActionType.Remove });
			if (returnedType.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedType);
				return;
			}
			var action = returnedType.Object;

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
			var returnedUser = Actions.GetGuildUser(Context, new[] { ObjectVerification.CanBeEdited }, true, userStr);
			if (returnedUser.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedUser);
				return;
			}
			var user = returnedUser.Object;

			//Get the botuser
			var botUser = ((List<BotImplementedPermissions>)guildInfo.GetSetting(SettingOnGuild.BotUsers)).FirstOrDefault(x => x.UserID == user.Id);
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

						((List<BotImplementedPermissions>)guildInfo.GetSetting(SettingOnGuild.BotUsers)).ThreadSafeRemove(botUser);
						guildInfo.SaveInfo();
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
				permissions.RemoveAll(x => Actions.CaseInsEquals(x.Name, GuildPermission.Administrator.EnumName()));
			}

			//Modify the user's perms
			botUser = botUser ?? new BotImplementedPermissions(user.Id, 0, guildInfo);
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
				((List<BotImplementedPermissions>)guildInfo.GetSetting(SettingOnGuild.BotUsers)).ThreadSafeRemoveAll(x => x.UserID == botUser.UserID);
			}

			guildInfo.SaveInfo();
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
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 2));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var settingStr = returnedArgs.Arguments[0];
			var chanStr = returnedArgs.Arguments[1];

			var returnedType = Actions.GetEnum(settingStr, new[] { ChannelSetting.ImageOnly, ChannelSetting.Sanitary });
			if (returnedType.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedType);
				return;
			}
			var type = returnedType.Object;

			var channel = Actions.GetChannel(Context, new[] { ObjectVerification.CanBeManaged, ObjectVerification.CanDeleteMessages, ObjectVerification.IsText }, true, input).Object ?? Context.Channel as ITextChannel;
			switch (type)
			{
				case ChannelSetting.ImageOnly:
				{
					var imgOnly = ((List<ulong>)guildInfo.GetSetting(SettingOnGuild.ImageOnlyChannels));
					if (imgOnly.Contains(channel.Id))
					{
						imgOnly.Remove(channel.Id);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed the channel `{0}` from the image only list."));
					}
					else
					{
						imgOnly.Add(channel.Id);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully added the channel `{0}` to the image only list."));
					}
					break;
				}
				case ChannelSetting.Sanitary:
				{
					var sanitary = ((List<ulong>)guildInfo.GetSetting(SettingOnGuild.SanitaryChannels));
					if (sanitary.Contains(channel.Id))
					{
						sanitary.Remove(channel.Id);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed the channel `{0}` from the sanitary list."));
					}
					else
					{
						sanitary.Add(channel.Id);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully added the channel `{0}` to the sanitary list."));
					}
					break;
				}
			}
		}

		[Command("modifyquotess")]
		[Alias("mrem")]
		[Usage("[Add|Remove] [\"Name\"] <\"Text\">")]
		[Summary("Adds the given text to a list that can be called through the `sayquote` command.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task ModifyQuotes([Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			//Split the input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 3));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];
			var nameStr = Actions.ReplaceMarkdownChars(returnedArgs.Arguments[1], true);
			var textStr = returnedArgs.Arguments[2];

			var returnedType = Actions.GetEnum(actionStr, new[] { ActionType.Add, ActionType.Remove });
			if (returnedType.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedType);
				return;
			}
			var action = returnedType.Object;
			var add = action == ActionType.Add;

			var quotes = ((List<Quote>)guildInfo.GetSetting(SettingOnGuild.Quotes));
			if (add)
			{
				if (quotes.Count >= Constants.MAX_QUOTES)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild already has the max number of quotes, which is 50."));
					return;
				}

				if (quotes.Any(x => Actions.CaseInsEquals(x.Name, nameStr)))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("A quote already has that name."));
					return;
				}

				//Make sure there's text
				if (String.IsNullOrWhiteSpace(textStr))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Adding a quote requires text."));
					return;
				}

				((List<Quote>)guildInfo.GetSetting(SettingOnGuild.Quotes)).Add(new Quote(nameStr, textStr));
			}
			else
			{
				if (!quotes.Any())
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("There needs to be at least one quote before you can remove any."));
					return;
				}

				//Remove all quotes with the same name
				quotes.RemoveAll(x => Actions.CaseInsEquals(x.Name, nameStr));
			}

			guildInfo.SaveInfo();
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} the following quote: `{1}`.", add ? "added" : "removed", nameStr));
		}

		[Command("sayquote")]
		[Alias("sq")]
		[Usage("<Name>")]
		[Summary("Shows the content for the given quote. If nothing is input, then shows the list of the current quotes.")]
		[OtherRequirement(Precondition.UserHasAPerm)]
		[DefaultEnabled(false)]
		public async Task SayQuote([Optional, Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);
			var quotes = ((List<Quote>)guildInfo.GetSetting(SettingOnGuild.Quotes));
			if (String.IsNullOrWhiteSpace(input))
			{
				if (!quotes.Any())
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("There are no quotes."));
				}
				else
				{
					await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("quotes", String.Format("`{0}`", String.Join("`, `", quotes.Select(x => x.Name)))));
				}
				return;
			}

			var quote = quotes.FirstOrDefault(x => Actions.CaseInsEquals(x.Name, input));
			if (quote != null)
			{
				await Actions.SendChannelMessage(Context, quote.Text);
				return;
			}

			var closeQuotes = Actions.GetQuotesWithSimilarNames(quotes, input).Distinct();
			if (closeQuotes.Any())
			{
				Variables.ActiveCloseWords.ThreadSafeRemoveAll(x => x.UserID == Context.User.Id);
				Variables.ActiveCloseWords.ThreadSafeAdd(new ActiveCloseWord<Quote>(Context.User.Id, closeQuotes));

				var msg = "Did you mean any of the following:\n" + closeQuotes.FormatNumberedList("{0}", x => x.Word.Name);
				await Actions.MakeAndDeleteSecondaryMessage(Context, msg, Constants.ACTIVE_CLOSE);
				return;
			}

			await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Nonexistent quote."));
		}

		[Command("setguildnotif")]
		[Alias("sgn")]
		[Usage("[Welcome|Goodbye] [#Channel] <\"Content:string\"> <\"Title:string\"> <\"Desc:string\"> <\"Thumb:string\">")]
		[Summary("The bot send a message to the given channel when the self explantory event happens. `{User}` will be replaced with the formatted user.  `{UserMention}` will be replaced with a mention of the joining user.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task SetGuildNotif([Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(3, 6), new[] { "content", "title", "desc", "thumb" });
			if (returnedArgs.Reason != FailureReason.NotFailure)
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
			var returnedType = Actions.GetEnum(typeStr, new[] { GuildNotificationType.Welcome, GuildNotificationType.Goodbye });
			if (returnedType.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedType);
				return;
			}
			var type = returnedType.Object;

			//Make sure the channel mention is valid
			var returnedChannel = Actions.GetChannel(Context, new[] { ObjectVerification.CanModifyPermissions, ObjectVerification.IsText }, true, chanStr);
			if (returnedChannel.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedChannel);
				return;
			}
			var channel = returnedChannel.Object as ITextChannel;

			var guildNotif = new GuildNotification(contStr, titleStr, descStr, thumbStr, Context.Guild.Id, channel.Id);
			switch (type)
			{
				case GuildNotificationType.Welcome:
				{
					if (guildInfo.SetSetting(SettingOnGuild.WelcomeMessage, guildNotif))
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully set the welcome message.");
					}
					else
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Failed to set the welcome message."));
					}
					break;
				}
				case GuildNotificationType.Goodbye:
				{
					if (guildInfo.SetSetting(SettingOnGuild.GoodbyeMessage, guildNotif))
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully set the goodbye message.");
					}
					else
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Failed to set the goodbye message."));
					}
					break;
				}
			}
		}

		[Command("testguildnotif")]
		[Alias("tgn")]
		[Usage("[Welcome|Goodbye]")]
		[Summary("Sends the given guild notification in order to test it.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task TestGuildNotification([Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			if (!Enum.TryParse(input, true, out GuildNotificationType notifType))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid notification type supplied."));
				return;
			}

			GuildNotification notif = null;
			switch (notifType)
			{
				case GuildNotificationType.Welcome:
				{
					notif = ((GuildNotification)guildInfo.GetSetting(SettingOnGuild.WelcomeMessage));
					break;
				}
				case GuildNotificationType.Goodbye:
				{
					notif = ((GuildNotification)guildInfo.GetSetting(SettingOnGuild.GoodbyeMessage));
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
