using Advobot.Actions;
using Advobot.TypeReaders;
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
using Advobot.SavedClasses;

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
		[Summary("Turns a command on or off. Can turn all commands in a category on or off too. Cannot turn off `" + nameof(ModifyCommands) + "` or `" + nameof(Miscellaneous.Help) + "`.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(true)]
		public sealed class ModifyCommands : MySavingModuleBase
		{
			private static readonly string[] _CommandsUnableToBeTurnedOff = new[] { nameof(ModifyCommands), nameof(Miscellaneous.Help) };

			[Group(nameof(ActionType.Enable)), Alias("e")]
			public sealed class Enable : MySavingModuleBase
			{
				[Command("all"), Priority(1)]
				public async Task CommandAll()
				{
					//Only grab commands that are already disabled and are able to be changed.
					var commands = Context.GuildSettings.CommandSwitches.Where(x => !x.Value && !_CommandsUnableToBeTurnedOff.CaseInsContains(x.Name));
					foreach (var command in commands)
					{
						command.ToggleEnabled();
					}
					var text = commands.Any() ? String.Join("`, `", commands.Select(x => x.Name)) : "None";
					await MessageActions.SendChannelMessage(Context, String.Format("Successfully enabled the following commands: `{0}`.", text));
				}
				[Command, Priority(0)]
				public async Task Command(CommandSwitch command)
				{
					if (command.Value)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("This command is already enabled."));
						return;
					}
					else if (_CommandsUnableToBeTurnedOff.CaseInsContains(command.Name))
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("Please don't try to edit that command."));
						return;
					}

					command.ToggleEnabled();
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully enabled `{0}`.", command.Name));
				}
				[Command]
				public async Task Command(CommandCategory category)
				{
					//Only grab commands that are already disabled and in the same category and are able to be changed.
					var commands = Context.GuildSettings.CommandSwitches.Where(x => !x.Value && x.Category == category && !_CommandsUnableToBeTurnedOff.CaseInsContains(x.Name));
					foreach (var command in commands)
					{
						command.ToggleEnabled();
					}
					await MessageActions.SendChannelMessage(Context, String.Format("Successfully enabled the following commands: `{0}`.", String.Join("`, `", commands.Select(x => x.Name))));
				}
			}
			[Group(nameof(ActionType.Disable)), Alias("d")]
			public sealed class Disable : MySavingModuleBase
			{
				[Command("all"), Priority(1)]
				public async Task CommandAll()
				{
					//Only grab commands that are already enabled and are able to be changed.
					var commands = Context.GuildSettings.CommandSwitches.Where(x => x.Value && !_CommandsUnableToBeTurnedOff.CaseInsContains(x.Name));
					foreach (var command in commands)
					{
						command.ToggleEnabled();
					}
					var text = commands.Any() ? String.Join("`, `", commands.Select(x => x.Name)) : "None";
					await MessageActions.SendChannelMessage(Context, String.Format("Successfully disabled the following commands: `{0}`.", text));
				}
				[Command, Priority(0)]
				public async Task Command(CommandSwitch command)
				{
					if (!command.Value)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("This command is already disabled."));
						return;
					}
					else if (_CommandsUnableToBeTurnedOff.CaseInsContains(command.Name))
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("Please don't try to edit that command."));
						return;
					}

					command.ToggleEnabled();
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully disabled `{0}`.", command.Name));
				}
				[Command]
				public async Task Command(CommandCategory category)
				{
					//Only grab commands that are already enabled and in the same category and are able to be changed.
					var commands = Context.GuildSettings.CommandSwitches.Where(x => x.Value && x.Category == category && !_CommandsUnableToBeTurnedOff.CaseInsContains(x.Name));
					foreach (var command in commands)
					{
						command.ToggleEnabled();
					}
					await MessageActions.SendChannelMessage(Context, String.Format("Successfully disabled the following commands: `{0}`.", String.Join("`, `", commands.Select(x => x.Name))));
				}
			}
		}

		[Group(nameof(ModifyIgnoredCommandChannels)), Alias("micc")]
		[Usage("[Add|Remove] [Channel] <Command Name|Category Name>")]
		[Summary("The bot will ignore commands said on these channels. If a command is input then the bot will instead ignore only that command on the given channel.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public sealed class ModifyIgnoredCommandChannels : MySavingModuleBase
		{
			private static readonly string[] _CommandsUnableToBeTurnedOff = new[] { nameof(ModifyIgnoredCommandChannels), nameof(Miscellaneous.Help) };

			[Group(nameof(ActionType.Add)), Alias("a")]
			public sealed class Add : MySavingModuleBase
			{
				[Command]
				public async Task Command([VerifyChannel(true, ChannelVerification.CanBeRead, ChannelVerification.CanBeEdited)] ITextChannel channel)
				{
					if (Context.GuildSettings.IgnoredCommandChannels.Contains(channel.Id))
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("`{0}` is already ignoring commands.", channel.FormatChannel()));
						return;
					}

					Context.GuildSettings.IgnoredCommandChannels.Add(channel.Id);
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully added `{0}` to the ignored command channels list.", channel.FormatChannel()));
				}
				[Command]
				public async Task Command([VerifyChannel(true, ChannelVerification.CanBeRead, ChannelVerification.CanBeEdited)] ITextChannel channel, CommandSwitch command)
				{
					//First remove every command override on that channel for that specific command (so whenever this command is used it forces an update on the status of the list, never have duplicates)
					Context.GuildSettings.CommandsDisabledOnChannel.RemoveAll(x => x.Id == channel.Id && x.Name.CaseInsEquals(command.Name));
					Context.GuildSettings.CommandsDisabledOnChannel.Add(new CommandOverride(command.Name, channel.Id));
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully started ignoring the command `{0}` on `{1}`.", command.Name, channel.FormatChannel()));
				}
				[Command]
				public async Task Command([VerifyChannel(true, ChannelVerification.CanBeRead, ChannelVerification.CanBeEdited)] ITextChannel channel, CommandCategory category)
				{
					var commands = Context.GuildSettings.CommandSwitches.Where(x => x.Category == category);
					foreach (var command in commands)
					{
						Context.GuildSettings.CommandsDisabledOnChannel.RemoveAll(x => x.Id == channel.Id && x.Name.CaseInsEquals(command.Name));
						Context.GuildSettings.CommandsDisabledOnChannel.Add(new CommandOverride(command.Name, channel.Id));
					}

					var cmdNames = String.Join("`, `", commands.Select(x => x.Name));
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully started ignoring the following commands on `{0}`: `{1}`.", channel.FormatChannel(), cmdNames));
				}
			}
			[Group(nameof(ActionType.Remove)), Alias("r")]
			public sealed class Remove : MySavingModuleBase
			{
				[Command]
				public async Task Command([VerifyChannel(true, ChannelVerification.CanBeRead, ChannelVerification.CanBeEdited)] ITextChannel channel)
				{
					if (!Context.GuildSettings.IgnoredCommandChannels.Contains(channel.Id))
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("`{0}` is already allowing commands.", channel.FormatChannel()));
						return;
					}

					Context.GuildSettings.IgnoredCommandChannels.RemoveAll(x => x == channel.Id);
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed `{0}` from the ignored command channels list.", channel.FormatChannel()));
				}
				[Command]
				public async Task Command([VerifyChannel(true, ChannelVerification.CanBeRead, ChannelVerification.CanBeEdited)] ITextChannel channel, CommandSwitch command)
				{
					//First remove every command override on that channel for that specific command (so whenever this command is used it forces an update on the status of the list, never have duplicates)
					Context.GuildSettings.CommandsDisabledOnChannel.RemoveAll(x => x.Id == channel.Id && x.Name.CaseInsEquals(command.Name));
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully stopped ignoring the command `{0}` on `{1}`.", command.Name, channel.FormatChannel()));
				}
				[Command]
				public async Task Command([VerifyChannel(true, ChannelVerification.CanBeRead, ChannelVerification.CanBeEdited)] ITextChannel channel, CommandCategory category)
				{
					var commands = Context.GuildSettings.CommandSwitches.Where(x => x.Category == category);
					foreach (var command in commands)
					{
						Context.GuildSettings.CommandsDisabledOnChannel.RemoveAll(x => x.Id == channel.Id && x.Name.CaseInsEquals(command.Name));
					}

					var cmdNames = String.Join("`, `", commands.Select(x => x.Name));
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully stopped ignoring the following commands on `{0}`: `{1}`.", channel.FormatChannel(), cmdNames));
				}
			}
		}

		[Group(nameof(ModifyBotUsers)), Alias("mbu")]
		[Usage("[Show|Add|Remove] <User> <Permission/...>")]
		[Summary("Gives a user permissions in the bot but not on Discord itself. Type `" + Constants.BOT_PREFIX + "mbu [Show]` to see the available permissions. " +
			"Type `" + Constants.BOT_PREFIX + "mbu [Show] [User]` to see the permissions of that user.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public sealed class ModifyBotUsers : MySavingModuleBase
		{
			[Group(nameof(ActionType.Show)), Alias("s")]
			public sealed class Show : MyModuleBase
			{
				[Command]
				public async Task Command()
				{
					await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Bot Permission Types", String.Format("`{0}`", String.Join("`, `", Constants.GUILD_PERMISSIONS.Select(x => x.Name)))));
				}
				[Command]
				public async Task Command(IUser user)
				{
					var botUser = Context.GuildSettings.BotUsers.FirstOrDefault(x => x.UserId == user.Id);
					if (botUser == null || botUser.Permissions == 0)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("That user has no extra permissions from the bot."));
						return;
					}

					var desc = String.Format("`{0}`", String.Join("`, `", GetActions.GetGuildPermissionNames(botUser.Permissions)));
					await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed(String.Format("Permissions for {0}", user.FormatUser()), desc));
				}
			}
			[Group(nameof(ActionType.Add)), Alias("a")]
			public sealed class Add : MySavingModuleBase
			{
				[Command]
				public async Task Command(IUser user, [Remainder, OverrideTypeReader(typeof(GuildPermissionsTypeReader))] ulong rawValue)
				{
					rawValue |= (Context.User as IGuildUser).GuildPermissions.RawValue;

					var botUser = Context.GuildSettings.BotUsers.FirstOrDefault(x => x.UserId == user.Id) ?? new BotImplementedPermissions(user.Id, rawValue);
					botUser.AddPermissions(rawValue);

					var givenPerms = String.Join("`, `", GetActions.GetGuildPermissionNames(rawValue));
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully gave `{0}` the following bot permissions: `{1}`.", user.FormatUser(), givenPerms));
				}
			}
			[Group(nameof(ActionType.Remove)), Alias("r")]
			public sealed class Remove : MySavingModuleBase
			{
				[Command]
				public async Task Command(IUser user, [Remainder, OverrideTypeReader(typeof(GuildPermissionsTypeReader))] ulong rawValue)
				{
					rawValue |= (Context.User as IGuildUser).GuildPermissions.RawValue;

					var botUser = Context.GuildSettings.BotUsers.FirstOrDefault(x => x.UserId == user.Id);
					if (botUser == null)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, "That user does not have any bot permissions to remove");
						return;
					}
					botUser.RemovePermissions(rawValue);

					var takenPerms = String.Join("`, `", GetActions.GetGuildPermissionNames(rawValue));
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed the following bot permissions from `{0}`: `{1}`.", user.FormatUser(), takenPerms));
				}
			}
		}

		[Group(nameof(ModifyPersistentRoles)), Alias("mpr")]
		[Usage("[Show|Add|Remove] [User] [Role]")]
		[Summary("Gives a user a role that stays even when they leave and rejoin the server.Type `" + Constants.BOT_PREFIX + "mpr [Show]` to see the which users have persistent roles set up. " +
			"Type `" + Constants.BOT_PREFIX + "mpr [Show] [User]` to see the persistent roles of that user.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public sealed class ModifyPersistentRoles : MySavingModuleBase
		{

		}

		[Group(nameof(ModifyChannelSettings)), Alias("mcs")]
		[Usage("[ImageOnly] <Channel>")]
		[Summary("Image only works solely on attachments. No input channel means it applies to the current channel. Using the command on an already targetted channel turns it off.")]
		[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
		[DefaultEnabled(false)]
		public sealed class ModifyChannelSettings : MySavingModuleBase
		{
			[Group(nameof(ChannelSetting.ImageOnly)), Alias("io")]
			public sealed class ImageOnly : MySavingModuleBase
			{
				[Command]
				public async Task Command([VerifyChannel(true, ChannelVerification.CanBeEdited)] ITextChannel channel)
				{
					if (Context.GuildSettings.ImageOnlyChannels.Contains(channel.Id))
					{
						Context.GuildSettings.ImageOnlyChannels.Remove(channel.Id);
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed the channel `{0}` from the image only list.", channel.FormatChannel()));
					}
					else
					{
						Context.GuildSettings.ImageOnlyChannels.Add(channel.Id);
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully added the channel `{0}` to the image only list.", channel.FormatChannel()));
					}
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
				var text = FormattingActions.FormatAllGuildSettings(Context.Guild, Context.GuildSettings);
				await UploadActions.WriteAndUploadTextFile(Context.Guild, Context.Channel, text, "Guild Settings", "Guild Settings");
			}
			[Command, Priority(0)]
			public async Task Command(string setting)
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
			[Command]
			public async Task Command()
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
