﻿using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Permissions;
using Advobot.Classes.TypeReaders;
using Advobot.Enums;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Advobot.Commands.GuildSettings
{
	[Group(nameof(ModifyGuildPrefix)), TopLevelShortAlias(nameof(ModifyGuildPrefix))]
	[Usage("[New Prefix|Clear]")]
	[Summary("Makes the bot use the given prefix in the guild.")]
	[OtherRequirement(Precondition.GuildOwner)]
	[DefaultEnabled(false)]
	public sealed class ModifyGuildPrefix : SavingModuleBase
	{
		[Command(nameof(Clear)), Priority(1)]
		public async Task Clear()
		{
			Context.GuildSettings.Prefix = null;
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully cleared the guild's prefix.");
		}
		[Command]
		public async Task Command([VerifyStringLength(Target.Prefix)] string newPrefix)
		{
			Context.GuildSettings.Prefix = newPrefix;
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set this guild's prefix to: `{newPrefix}`.");
		}
	}

	[Group(nameof(ModifyCommands)), TopLevelShortAlias(nameof(ModifyCommands))]
	[Usage("[Enable|Disable] [Command Name|Category Name|All]")]
	[Summary("Turns a command on or off. Can turn all commands in a category on or off too. " +
		"Cannot turn off `" + nameof(ModifyCommands) + "` or `" + nameof(Miscellaneous.Help) + "`.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyCommands : SavingModuleBase
	{
		private static readonly string[] _CommandsUnableToBeTurnedOff = new[] { nameof(ModifyCommands), nameof(Miscellaneous.Help) };

		[Group(nameof(Enable)), ShortAlias(nameof(Enable))]
		public sealed class Enable : SavingModuleBase
		{
			[Command(nameof(All)), ShortAlias(nameof(All)), Priority(1)]
			public async Task All()
			{
				//Only grab commands that are already disabled and are able to be changed.
				var commands = Context.GuildSettings.CommandSwitches.Where(x => !x.Value && !_CommandsUnableToBeTurnedOff.CaseInsContains(x.Name)).ToArray();
				foreach (var command in commands)
				{
					command.ToggleEnabled();
				}
				var text = commands.Any() ? String.Join("`, `", commands.Select(x => x.Name)) : "None";
				await MessageActions.SendMessage(Context.Channel, $"Successfully enabled the following commands: `{text}`.");
			}
			[Command]
			public async Task Command(CommandSwitch command)
			{
				if (command.Value)
				{
					await MessageActions.SendErrorMessage(Context, new ErrorReason("This command is already enabled."));
					return;
				}
				else if (_CommandsUnableToBeTurnedOff.CaseInsContains(command.Name))
				{
					await MessageActions.SendErrorMessage(Context, new ErrorReason("Please don't try to edit that command."));
					return;
				}

				command.ToggleEnabled();
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully enabled `{command.Name}`.");
			}
			[Command]
			public async Task Command(CommandCategory category)
			{
				//Only grab commands that are already disabled and in the same category and are able to be changed.
				var commands = Context.GuildSettings.GetCommands(category).Where(x => !x.Value && !_CommandsUnableToBeTurnedOff.CaseInsContains(x.Name));
				foreach (var command in commands)
				{
					command.ToggleEnabled();
				}
				var text = commands.Any() ? String.Join("`, `", commands.Select(x => x.Name)) : "None";
				await MessageActions.SendMessage(Context.Channel, $"Successfully enabled the following commands: `{text}`.");
			}
		}
		[Group(nameof(Disable)), ShortAlias(nameof(Disable))]
		public sealed class Disable : SavingModuleBase
		{
			[Command(nameof(All)), ShortAlias(nameof(All)), Priority(1)]
			public async Task All()
			{
				//Only grab commands that are already enabled and are able to be changed.
				var commands = Context.GuildSettings.CommandSwitches.Where(x => x.Value && !_CommandsUnableToBeTurnedOff.CaseInsContains(x.Name)).ToArray();
				foreach (var command in commands)
				{
					command.ToggleEnabled();
				}
				var text = commands.Any() ? String.Join("`, `", commands.Select(x => x.Name)) : "None";
				await MessageActions.SendMessage(Context.Channel, $"Successfully disabled the following commands: `{text}`.");
			}
			[Command]
			public async Task Command(CommandSwitch command)
			{
				if (!command.Value)
				{
					await MessageActions.SendErrorMessage(Context, new ErrorReason("This command is already disabled."));
					return;
				}
				else if (_CommandsUnableToBeTurnedOff.CaseInsContains(command.Name))
				{
					await MessageActions.SendErrorMessage(Context, new ErrorReason("Please don't try to edit that command."));
					return;
				}

				command.ToggleEnabled();
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully disabled `{command.Name}`.");
			}
			[Command]
			public async Task Command(CommandCategory category)
			{
				//Only grab commands that are already enabled and in the same category and are able to be changed.
				var commands = Context.GuildSettings.GetCommands(category).Where(x => x.Value && !_CommandsUnableToBeTurnedOff.CaseInsContains(x.Name));
				foreach (var command in commands)
				{
					command.ToggleEnabled();
				}
				var text = commands.Any() ? String.Join("`, `", commands.Select(x => x.Name)) : "None";
				await MessageActions.SendMessage(Context.Channel, $"Successfully disabled the following commands: `{text}`.");
			}
		}
	}

	[Group(nameof(ModifyIgnoredCommandChannels)), TopLevelShortAlias(nameof(ModifyIgnoredCommandChannels))]
	[Usage("[Add|Remove] [Channel] <Command Name|Category Name>")]
	[Summary("The bot will ignore commands said on these channels. If a command is input then the bot will instead ignore only that command on the given channel.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyIgnoredCommandChannels : SavingModuleBase
	{
		private static readonly string[] _CommandsUnableToBeTurnedOff = new[] { nameof(ModifyIgnoredCommandChannels), nameof(Miscellaneous.Help) };

		[Group(nameof(Add)), ShortAlias(nameof(Add))]
		public sealed class Add : SavingModuleBase
		{
			[Command]
			public async Task Command([VerifyObject(true, ObjectVerification.CanBeRead, ObjectVerification.CanBeEdited)] ITextChannel channel)
			{
				if (Context.GuildSettings.IgnoredCommandChannels.Contains(channel.Id))
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"`{channel.FormatChannel()}` is already ignoring commands.");
					return;
				}

				Context.GuildSettings.IgnoredCommandChannels.Add(channel.Id);
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully added `{channel.FormatChannel()}` to the ignored command channels list.");
			}
			[Command]
			public async Task Command([VerifyObject(true, ObjectVerification.CanBeRead, ObjectVerification.CanBeEdited)] ITextChannel channel, CommandSwitch command)
			{
				//First remove every command override on that channel for that specific command (so whenever this command is used it forces an update on the status of the list, never have duplicates)
				Context.GuildSettings.CommandsDisabledOnChannel.RemoveAll(x => x.Id == channel.Id && x.Name.CaseInsEquals(command.Name));
				Context.GuildSettings.CommandsDisabledOnChannel.Add(new CommandOverride(command.Name, channel.Id));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully started ignoring the command `{command.Name}` on `{channel.FormatChannel()}`.");
			}
			[Command]
			public async Task Command([VerifyObject(true, ObjectVerification.CanBeRead, ObjectVerification.CanBeEdited)] ITextChannel channel, CommandCategory category)
			{
				var commands = Context.GuildSettings.GetCommands(category);
				foreach (var command in commands)
				{
					Context.GuildSettings.CommandsDisabledOnChannel.RemoveAll(x => x.Id == channel.Id && x.Name.CaseInsEquals(command.Name));
					Context.GuildSettings.CommandsDisabledOnChannel.Add(new CommandOverride(command.Name, channel.Id));
				}

				var cmdNames = String.Join("`, `", commands.Select(x => x.Name));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully started ignoring the following commands on `{channel.FormatChannel()}`: `{cmdNames}`.");
			}
		}
		[Group(nameof(Remove)), ShortAlias(nameof(Remove))]
		public sealed class Remove : SavingModuleBase
		{
			[Command]
			public async Task Command([VerifyObject(true, ObjectVerification.CanBeRead, ObjectVerification.CanBeEdited)] ITextChannel channel)
			{
				if (!Context.GuildSettings.IgnoredCommandChannels.Contains(channel.Id))
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"`{channel.FormatChannel()}` is already allowing commands.");
					return;
				}

				Context.GuildSettings.IgnoredCommandChannels.RemoveAll(x => x == channel.Id);
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully removed `{channel.FormatChannel()}` from the ignored command channels list.");
			}
			[Command]
			public async Task Command([VerifyObject(true, ObjectVerification.CanBeRead, ObjectVerification.CanBeEdited)] ITextChannel channel, CommandSwitch command)
			{
				//First remove every command override on that channel for that specific command (so whenever this command is used it forces an update on the status of the list, never have duplicates)
				Context.GuildSettings.CommandsDisabledOnChannel.RemoveAll(x => x.Id == channel.Id && x.Name.CaseInsEquals(command.Name));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully stopped ignoring the command `{command.Name}` on `{channel.FormatChannel()}`.");
			}
			[Command]
			public async Task Command([VerifyObject(true, ObjectVerification.CanBeRead, ObjectVerification.CanBeEdited)] ITextChannel channel, CommandCategory category)
			{
				var commands = Context.GuildSettings.GetCommands(category);
				foreach (var command in commands)
				{
					Context.GuildSettings.CommandsDisabledOnChannel.RemoveAll(x => x.Id == channel.Id && x.Name.CaseInsEquals(command.Name));
				}

				var cmdNames = String.Join("`, `", commands.Select(x => x.Name));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully stopped ignoring the following commands on `{channel.FormatChannel()}`: `{cmdNames}`.");
			}
		}
	}

	[Group(nameof(ModifyBotUsers)), TopLevelShortAlias(nameof(ModifyBotUsers))]
	[Usage("[Show|Add|Remove] <User> <Permission/...>")]
	[Summary("Gives a user permissions in the bot but not on Discord itself. " +
		"Type `" + nameof(ModifyBotUsers) + " [Show]` to see the available permissions. " +
		"Type `" + nameof(ModifyBotUsers) + " [Show] [User]` to see the permissions of that user.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyBotUsers : SavingModuleBase
	{
		[Group(nameof(Show)), ShortAlias(nameof(Show))]
		public sealed class Show : AdvobotModuleBase
		{
			[Command]
			public async Task Command()
			{
				var desc = $"`{String.Join("`, `", GuildPerms.Permissions.Select(x => x.Name))}`";
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Bot Permission Types", desc));
			}
			[Command]
			public async Task Command(IUser user)
			{
				var botUser = Context.GuildSettings.BotUsers.FirstOrDefault(x => x.UserId == user.Id);
				if (botUser == null || botUser.Permissions == 0)
				{
					await MessageActions.SendErrorMessage(Context, new ErrorReason("That user has no extra permissions from the bot."));
					return;
				}

				var desc = $"`{String.Join("`, `", GuildPerms.ConvertValueToNames(botUser.Permissions))}`";
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed($"Permissions for {user.FormatUser()}", desc));
			}
		}
		[Group(nameof(Add)), ShortAlias(nameof(Add))]
		public sealed class Add : SavingModuleBase
		{
			[Command]
			public async Task Command(IUser user, [Remainder, OverrideTypeReader(typeof(GuildPermissionsTypeReader))] ulong rawValue)
			{
				rawValue |= (Context.User as IGuildUser).GuildPermissions.RawValue;

				var botUser = Context.GuildSettings.BotUsers.FirstOrDefault(x => x.UserId == user.Id) ?? new BotImplementedPermissions(user.Id, rawValue);
				botUser.AddPermissions(rawValue);

				var givenPerms = String.Join("`, `", GuildPerms.ConvertValueToNames(rawValue));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully gave `{user.FormatUser()}` the following bot permissions: `{givenPerms}`.");
			}
		}
		[Group(nameof(Remove)), ShortAlias(nameof(Remove))]
		public sealed class Remove : SavingModuleBase
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

				var takenPerms = String.Join("`, `", GuildPerms.ConvertValueToNames(rawValue));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully removed the following bot permissions from `{user.FormatUser()}`: `{takenPerms}`.");
			}
		}
	}

	[Group(nameof(ModifyPersistentRoles)), TopLevelShortAlias(nameof(ModifyPersistentRoles))]
	[Usage("[Show|Add|Remove] [User] [Role]")]
	[Summary("Gives a user a role that stays even when they leave and rejoin the server." +
		"Type `" + nameof(ModifyPersistentRoles) + " [Show]` to see the which users have persistent roles set up. " +
		"Type `" + nameof(ModifyPersistentRoles) + " [Show] [User]` to see the persistent roles of that user.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyPersistentRoles : SavingModuleBase
	{

	}

	[Group(nameof(ModifyChannelSettings)), TopLevelShortAlias(nameof(ModifyChannelSettings))]
	[Usage("[ImageOnly] <Channel>")]
	[Summary("Image only works solely on attachments. No input channel means it applies to the current channel. Using the command on an already targetted channel turns it off.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyChannelSettings : SavingModuleBase
	{
		[Command(nameof(ImageOnly)), ShortAlias(nameof(ImageOnly))]
		public async Task ImageOnly([VerifyObject(true, ObjectVerification.CanBeEdited)] ITextChannel channel)
		{
			if (Context.GuildSettings.ImageOnlyChannels.Contains(channel.Id))
			{
				Context.GuildSettings.ImageOnlyChannels.Remove(channel.Id);
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully removed the channel `{channel.FormatChannel()}` from the image only list.");
			}
			else
			{
				Context.GuildSettings.ImageOnlyChannels.Add(channel.Id);
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully added the channel `{channel.FormatChannel()}` to the image only list.");
			}
		}
	}

	[Group(nameof(ModifyGuildNotifs)), TopLevelShortAlias(nameof(ModifyGuildNotifs))]
	[Usage("[Welcome|Goodbye] [Channel] <\"Content:string\"> <\"Title:string\"> <\"Desc:string\"> <\"Thumb:string\">")]
	[Summary("The bot send a message to the given channel when the self explantory event happens. " +
		"`" + Constants.USER_MENTION + "` will be replaced with the formatted user. " +
		"`" + Constants.USER_STRING + "` will be replaced with a mention of the joining user.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyGuildNotifs : SavingModuleBase
	{
		[Command(nameof(Welcome)), ShortAlias(nameof(Welcome))]
		public async Task Welcome([VerifyObject(true, ObjectVerification.CanModifyPermissions)] ITextChannel channel, [Remainder] string input)
		{
			var inputArgs = input.SplitExceptInQuotes().ToList();
			var content = GetActions.GetVariableAndRemove(inputArgs, "content");
			var title = GetActions.GetVariableAndRemove(inputArgs, "title");
			var desc = GetActions.GetVariableAndRemove(inputArgs, "desc");
			var thumb = GetActions.GetVariableAndRemove(inputArgs, "thumb");

			Context.GuildSettings.WelcomeMessage = new GuildNotification(content, title, desc, thumb, channel.Id);
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully set the welcome message.");
		}
		[Command(nameof(Goodbye)), ShortAlias(nameof(Goodbye))]
		public async Task Goodbye([VerifyObject(true, ObjectVerification.CanModifyPermissions)] ITextChannel channel, [Remainder] string input)
		{
			var inputArgs = input.SplitExceptInQuotes().ToList();
			var content = GetActions.GetVariableAndRemove(inputArgs, "content");
			var title = GetActions.GetVariableAndRemove(inputArgs, "title");
			var desc = GetActions.GetVariableAndRemove(inputArgs, "desc");
			var thumb = GetActions.GetVariableAndRemove(inputArgs, "thumb");

			Context.GuildSettings.GoodbyeMessage = new GuildNotification(content, title, desc, thumb, channel.Id);
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully set the goodbye message.");
		}
	}

	[Group(nameof(TestGuildNotifs)), TopLevelShortAlias(nameof(TestGuildNotifs))]
	[Usage("[Welcome|Goodbye]")]
	[Summary("Sends the given guild notification in order to test it.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class TestGuildNotifs : AdvobotModuleBase
	{
		[Command(nameof(Welcome)), ShortAlias(nameof(Welcome))]
		public async Task Welcome()
		{
			var notif = Context.GuildSettings.WelcomeMessage;
			if (notif == null)
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("The welcome notification does not exist."));
				return;
			}

			await notif.Send(null);
		}
		[Command(nameof(Goodbye)), ShortAlias(nameof(Goodbye))]
		public async Task Goodbye()
		{
			var notif = Context.GuildSettings.GoodbyeMessage;
			if (notif == null)
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("The goodbye notification does not exist."));
				return;
			}

			await notif.Send(null);
		}
	}

	[Group(nameof(DisplayGuildSettings)), TopLevelShortAlias(nameof(DisplayGuildSettings))]
	[Usage("[Show|All|Setting Name]")]
	[Summary("Displays guild settings. Show gives a list of the setting names.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(true)]
	public sealed class DisplayGuildSettings : AdvobotModuleBase
	{
		[Command(nameof(Show)), ShortAlias(nameof(Show)), Priority(1)]
		public async Task Show()
		{
			var desc = $"`{String.Join("`, `", GetActions.GetGuildSettings().Select(x => x.Name))}`";
			await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Setting Names", desc));
		}
		[Command(nameof(All)), ShortAlias(nameof(All)), Priority(1)]
		public async Task All()
		{
			var text = Context.GuildSettings.ToString();
			await MessageActions.SendTextFile(Context.Channel, text, "Guild_Settings", "Guild Settings");
		}
		[Command]
		public async Task Command([OverrideTypeReader(typeof(SettingTypeReader.GuildSettingTypeReader))] PropertyInfo setting)
		{
			var desc = Context.GuildSettings.ToString(setting);
			if (desc.Length <= Constants.MAX_DESCRIPTION_LENGTH)
			{
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed(setting.Name, desc));
			}
			else
			{
				await MessageActions.SendTextFile(Context.Channel, desc, setting.Name, setting.Name);
			}
		}
	}

	[Group(nameof(GetFile)), TopLevelShortAlias(nameof(GetFile))]
	[Usage("")]
	[Summary("Sends the file containing all the guild's saved bot information.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class GetFile : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			var file = GetActions.GetServerDirectoryFile(Context.Guild.Id, Constants.GUILD_SETTINGS_LOCATION);
			if (!file.Exists)
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("The guild information file does not exist at this time."));
				return;
			}
			await Context.Channel.SendFileAsync(file.FullName);
		}
	}
}
