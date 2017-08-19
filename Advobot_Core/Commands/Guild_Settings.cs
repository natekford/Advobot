using Advobot.Actions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.NonSavedClasses;
using Advobot.Enums;
using Advobot.Attributes;
using System.Reflection;
using Advobot.Interfaces;

namespace Advobot
{
	namespace GuildSettings
	{
		[Group(nameof(ModifyGuildPrefix)), Alias("mgdp")]
		[Usage("[New Prefix|Clear]")]
		[Summary("Makes the bot use the given prefix in the guild.")]
		[OtherRequirement(Precondition.GuildOwner)]
		[DefaultEnabled(false)]
		public sealed class ModifyGuildPrefix : MySavingModuleBase
		{
			[Command(nameof(ActionType.Clear)), Priority(1)]
			public async Task Command()
			{
				Context.GuildSettings.Prefix = null;
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully cleared the guild's prefix.");
			}
			[Command, Priority(0)]
			public async Task Command([VerifyStringLength(Target.Prefix)] string newPrefix)
			{
				Context.GuildSettings.Prefix = newPrefix;
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set this guild's prefix to: `{0}`.", newPrefix));
			}
		}

		[Group(nameof(ModifyCommands)), Alias("modcom", "mcom")]
		[Usage("[Enable|Disable] [Command Name|Category Name|All]")]
		[Summary("Turns a command on or off. Can turn all commands in a category on or off too. Cannot turn off `configurecommands` or `help`.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(true)]
		public sealed class ModifyCommands : MySavingModuleBase
		{
			[Group(nameof(ActionType.Enable)), Alias("e")]
			public sealed class Enable : MySavingModuleBase
			{
				[Command("all"), Priority(1)]
				public async Task CommandAll()
				{
					await CommandRunnerAll();
				}
				[Command, Priority(0)]
				public async Task Command(string commandName)
				{
					await CommandRunner(commandName);
				}
				[Command]
				public async Task Command(CommandCategory category)
				{
					await CommandRunner(category);
				}

				private async Task CommandRunnerAll()
				{

				}
				private async Task CommandRunner(string commandName)
				{

				}
				private async Task CommandRunner(CommandCategory category)
				{

				}
			}
			[Group(nameof(ActionType.Disable)), Alias("d")]
			public sealed class Disable : MySavingModuleBase
			{
				[Command("all"), Priority(1)]
				public async Task CommandAll()
				{
					await CommandRunnerAll();
				}
				[Command, Priority(0)]
				public async Task Command(string commandName)
				{
					await CommandRunner(commandName);
				}
				[Command]
				public async Task Command(CommandCategory category)
				{
					await CommandRunner(category);
				}

				private async Task CommandRunnerAll()
				{

				}
				private async Task CommandRunner(string commandName)
				{

				}
				private async Task CommandRunner(CommandCategory category)
				{

				}
			}
		}

		[Group(nameof(DisplayGuildSettings)), Alias("dgds")]
		[Usage("<All|Setting Name>")]
		[Summary("Displays guild settings. Inputting nothing gives a list of the setting names.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(true)]
		public sealed class DisplayGuildSettings : MyModuleBase
		{
			[Command("all"), Priority(1)]
			public async Task CommandAll()
			{
				await CommandRunnerAll();
			}
			[Command, Priority(0)]
			public async Task Command(string setting)
			{
				await CommandRunner(setting);
			}
			[Command]
			public async Task Command()
			{
				await CommandRunner();
			}

			private async Task CommandRunnerAll()
			{
				var text = FormattingActions.FormatAllGuildSettings(Context.Guild, Context.GuildSettings);
				await UploadActions.WriteAndUploadTextFile(Context.Guild, Context.Channel, text, "Guild Settings", "Guild Settings");
			}
			private async Task CommandRunner(string setting)
			{
				var currentSetting = GetActions.GetGuildSettings().FirstOrDefault(x => x.Name.CaseInsEquals(setting));
				if (currentSetting == null)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("Unable to find a setting with the supplied name."));
					return;
				}

				var desc = FormattingActions.FormatGuildSettingInfo(Context.Guild as SocketGuild, Context.GuildSettings, currentSetting);
				if (desc.Length <= Constants.MAX_DESCRIPTION_LENGTH)
				{
					await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed(currentSetting.Name, desc));
				}
				else
				{
					await UploadActions.WriteAndUploadTextFile(Context.Guild, Context.Channel, desc, currentSetting.Name, currentSetting.Name);
				}
			}
			private async Task CommandRunner()
			{
				var settingNames = GetActions.GetGuildSettings().Select(x => x.Name);

				var desc = String.Format("`{0}`", String.Join("`, `", settingNames));
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Setting Names", desc));
			}
		}

		[Group(nameof(GetFile)), Alias("gf")]
		[Usage("")]
		[Summary("Sends the file containing all the guild's saved bot information.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public sealed class GetFile : MyModuleBase
		{
			[Command]
			public async Task Command()
			{
				await CommandRunner();
			}

			private async Task CommandRunner()
			{
				var file = GetActions.GetServerDirectoryFile(Context.Guild.Id, Constants.GUILD_SETTINGS_LOCATION);
				if (!file.Exists)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("The guild information file does not exist at this time."));
					return;
				}
				await UploadActions.UploadFile(Context.Channel, file);
			}
		}
	}
	/*
	//Guild Settings commands are commands that only affect that specific guild
	[Name("GuildSettings")]
	public class Advobot_Commands_Guild_Settings : ModuleBase
	{


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
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("No command or category has that name."));
					return;
				}
			}

			switch (action)
			{
				case ActionType.Enable:
				{
					if (command != null && command.ValAsBoolean)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("This command is already enabled."));
						return;
					}
					break;
				}
				case ActionType.Disable:
				{
					if (command != null && !command.ValAsBoolean)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("This command is already disabled."));
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
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Please don't try to edit that command."));
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
			await MessageActions.SendChannelMessage(Context, Actions.FormatResponseMessagesForCmdsOnLotsOfObjects(commands.Select(x => x.Name), unableToBeRemoved.Select(x => x.Name), "command", pastTense, presentTense));
		}

		[Command("modifyignoredcommandchannels")]
		[Alias("micc")]
		[Usage("[Add|Remove] [Channel] <Command Name|Category Name>")]
		[Summary("The bot will ignore commands said on these channels. If a command is input then the bot will instead ignore only that command on the given channel.")]
		[PermissionRequirement(null, null)]
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
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("The given input `{0}` is not a valid command or category.", cmdStr));
					return;
				}

				if (cmd != null)
				{
					if (add)
					{
						if (ignoredCmdsOnChans.Any(x => x.Name == cmd && x.ID == channel.Id))
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("This command is already ignored on this channel."));
							return;
						}
						ignoredCmdsOnChans.Add(new CommandOverride(cmd, channel.Id, false));
					}
					else
					{
						var amtRemoved = ignoredCmdsOnChans.RemoveAll(x => x.Name == cmd && x.ID == channel.Id);
						if (amtRemoved == 0)
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("This command is already not ignored on this channel."));
							return;
						}
					}
					await MessageActions.MakeAndDeleteSecondaryMessage(Context,
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
					await MessageActions.MakeAndDeleteSecondaryMessage(Context,
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
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("This channel is already ignored for commands."));
						return;
					}
					ignoredCmdChannels.Add(channel.Id);
				}
				else
				{
					if (!ignoredCmdChannels.Contains(channel.Id))
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("This channel is already not ignored for commands."));
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
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, outputStr);
			}
		}

		[Command("modifybotusers")]
		[Alias("mbu")]
		[Usage("[Show|Add|Remove] [User] [Permission/...]")]
		[Summary("Gives a user permissions in the bot but not on Discord itself. Type `" + Constants.BOT_PREFIX + "mbu [Show]` to see the available permissions. " +
			"Type `" + Constants.BOT_PREFIX + "mbu [Show] [User]` to see the permissions of that user.")]
		[PermissionRequirement(null, null)]
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
					await MessageActions.SendEmbedMessage(Context.Channel, Messages.MakeNewEmbed("Guild Permissions", String.Format("`{0}`", String.Join("`, `", Variables.GuildPermissions.Select(x => x.Name)))));
					return;
				}
				else
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR(Constants.ARGUMENTS_ERROR));
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
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("That user has no extra permissions from the bot."));
							return;
						}

						//check if that user has any permissions
						var showPerms = Actions.GetPermissionNames(botUser.Permissions);
						if (!showPerms.Any())
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("That user has no extra permissions from the bot."));
						}
						else
						{
							var title = String.Format("Permissions for {0}", user.FormatUser());
							var desc = String.Format("`{0}`", String.Join("`, `", showPerms));
							await MessageActions.SendEmbedMessage(Context.Channel, Messages.MakeNewEmbed(title, desc));
						}
					}
					else
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Invalid option for show."));
					}
					return;
				}
				case ActionType.Remove:
				{
					if (returnedArgs.ArgCount == 2)
					{
						if (botUser == null)
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("That user is not on the bot user list."));
							return;
						}

						((List<BotImplementedPermissions>)guildInfo.GetSetting(SettingOnGuild.BotUsers)).ThreadSafeRemove(botUser);
						guildInfo.SaveInfo();
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed `{0}` from the bot user list.", user.FormatUser()));
						return;
					}
					break;
				}
			}

			//Get the permissions
			var permissions = permStr.Split('/').Select(x => Variables.GuildPermissions.FirstOrDefault(y => Actions.CaseInsEquals(y.Name, x))).Where(x => x.Name != null).ToList();
			if (!permissions.Any())
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Invalid input for permissions."));
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
						botUser.AddPermission(x.Bit);
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
						botUser.RemovePermission(x.Bit);
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
			await MessageActions.SendChannelMessage(Context, String.Format("Successfully {0}: `{1}`.", outputStr, String.Join("`, `", permissions.Select(x => x.Name))));
		}

		[Command("modifychannelsettings")]
		[Alias("mcs")]
		[Usage("[ImageOnly|Sanitary] <Channel>")]
		[Summary("Image only works solely on attachments. Sanitary means any message sent by someone without admin gets deleted. " +
			"No input channel means it applies to the current channel. Using the command on an already targetted channel turned it off.")]
		[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
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
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed the channel `{0}` from the image only list."));
					}
					else
					{
						imgOnly.Add(channel.Id);
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully added the channel `{0}` to the image only list."));
					}
					break;
				}
				case ChannelSetting.Sanitary:
				{
					var sanitary = ((List<ulong>)guildInfo.GetSetting(SettingOnGuild.SanitaryChannels));
					if (sanitary.Contains(channel.Id))
					{
						sanitary.Remove(channel.Id);
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed the channel `{0}` from the sanitary list."));
					}
					else
					{
						sanitary.Add(channel.Id);
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully added the channel `{0}` to the sanitary list."));
					}
					break;
				}
			}
		}

		[Command("modifyquotess")]
		[Alias("mrem")]
		[Usage("[Add|Remove] [\"Name\"] <\"Text\">")]
		[Summary("Adds the given text to a list that can be called through the `sayquote` command.")]
		[PermissionRequirement(null, null)]
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
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("This guild already has the max number of quotes, which is 50."));
					return;
				}

				if (quotes.Any(x => Actions.CaseInsEquals(x.Name, nameStr)))
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("A quote already has that name."));
					return;
				}

				//Make sure there's text
				if (String.IsNullOrWhiteSpace(textStr))
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Adding a quote requires text."));
					return;
				}

				((List<Quote>)guildInfo.GetSetting(SettingOnGuild.Quotes)).Add(new Quote(nameStr, textStr));
			}
			else
			{
				if (!quotes.Any())
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("There needs to be at least one quote before you can remove any."));
					return;
				}

				//Remove all quotes with the same name
				quotes.RemoveAll(x => Actions.CaseInsEquals(x.Name, nameStr));
			}

			guildInfo.SaveInfo();
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} the following quote: `{1}`.", add ? "added" : "removed", nameStr));
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
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("There are no quotes."));
				}
				else
				{
					await MessageActions.SendEmbedMessage(Context.Channel, Messages.MakeNewEmbed("quotes", String.Format("`{0}`", String.Join("`, `", quotes.Select(x => x.Name)))));
				}
				return;
			}

			var quote = quotes.FirstOrDefault(x => Actions.CaseInsEquals(x.Name, input));
			if (quote != null)
			{
				await MessageActions.SendChannelMessage(Context, quote.Text);
				return;
			}

			var closeQuotes = Actions.GetQuotesWithSimilarNames(quotes, input).Distinct();
			if (closeQuotes.Any())
			{
				Variables.ActiveCloseWords.ThreadSafeRemoveAll(x => x.UserID == Context.User.Id);
				Variables.ActiveCloseWords.ThreadSafeAdd(new ActiveCloseWord<Quote>(Context.User.Id, closeQuotes));

				var msg = "Did you mean any of the following:\n" + closeQuotes.FormatNumberedList("{0}", x => x.Word.Name);
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, msg, Constants.ACTIVE_CLOSE);
				return;
			}

			await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Nonexistent quote."));
		}

		[Command("setguildnotif")]
		[Alias("sgn")]
		[Usage("[Welcome|Goodbye] [#Channel] <\"Content:string\"> <\"Title:string\"> <\"Desc:string\"> <\"Thumb:string\">")]
		[Summary("The bot send a message to the given channel when the self explantory event happens. `{User}` will be replaced with the formatted user.  `{UserMention}` will be replaced with a mention of the joining user.")]
		[PermissionRequirement(null, null)]
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
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("One of the variables has to be given."));
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
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully set the welcome message.");
					}
					else
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Failed to set the welcome message."));
					}
					break;
				}
				case GuildNotificationType.Goodbye:
				{
					if (guildInfo.SetSetting(SettingOnGuild.GoodbyeMessage, guildNotif))
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully set the goodbye message.");
					}
					else
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Failed to set the goodbye message."));
					}
					break;
				}
			}
		}

		[Command("testguildnotif")]
		[Alias("tgn")]
		[Usage("[Welcome|Goodbye]")]
		[Summary("Sends the given guild notification in order to test it.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public async Task TestGuildNotification([Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			if (!Enum.TryParse(input, true, out GuildNotificationType notifType))
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Invalid notification type supplied."));
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
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("The notification does not exist."));
				return;
			}

			await Actions.SendGuildNotification(null, notif);
		}
	}
	*/
}
