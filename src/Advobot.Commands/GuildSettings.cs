using Advobot.Core;
using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.Permissions;
using Advobot.Core.Classes.TypeReaders;
using Advobot.Core.Enums;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Advobot.Commands.GuildSettings
{
	[Group(nameof(ModifyGuildPrefix)), TopLevelShortAlias(typeof(ModifyGuildPrefix))]
	[Summary("Makes the bot use the given prefix in the guild.")]
	[OtherRequirement(Precondition.GuildOwner)]
	[DefaultEnabled(false)]
	public sealed class ModifyGuildPrefix : SavingModuleBase
	{
		[Command(nameof(Clear)), Priority(1)]
		public async Task Clear()
		{
			Context.GuildSettings.Prefix = null;
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully cleared the guild's prefix.").CAF();
		}
		[Command]
		public async Task Command([VerifyStringLength(Target.Prefix)] string newPrefix)
		{
			Context.GuildSettings.Prefix = newPrefix;
			var resp = $"Successfully set this guild's prefix to: `{Context.GuildSettings.Prefix}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifyCommands)), TopLevelShortAlias(typeof(ModifyCommands))]
	[Summary("Turns a command on or off. " +
		"Can turn all commands in a category on or off too. " +
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
				var commands = Context.GuildSettings.CommandSwitches.Where(x =>
				{
					return !x.Value && !_CommandsUnableToBeTurnedOff.CaseInsContains(x.Name);
				}).ToArray();
				foreach (var command in commands)
				{
					command.ToggleEnabled();
				}
				var text = commands.Any() ? String.Join("`, `", commands.Select(x => x.Name)) : "None";
				await MessageUtils.SendMessageAsync(Context.Channel, $"Successfully enabled the following commands: `{text}`.").CAF();
			}
			[Command]
			public async Task Command(CommandSwitch command)
			{
				if (command.Value)
				{
					await MessageUtils.SendErrorMessageAsync(Context, new ErrorReason("This command is already enabled.")).CAF();
					return;
				}
				else if (_CommandsUnableToBeTurnedOff.CaseInsContains(command.Name))
				{
					await MessageUtils.SendErrorMessageAsync(Context, new ErrorReason("Please don't try to edit that command.")).CAF();
					return;
				}

				command.ToggleEnabled();
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully enabled `{command.Name}`.").CAF();
			}
			[Command]
			public async Task Command(CommandCategory category)
			{
				//Only grab commands that are already disabled and in the same category and are able to be changed.
				var commands = Context.GuildSettings.GetCommands(category).Where(x =>
				{
					return !x.Value && !_CommandsUnableToBeTurnedOff.CaseInsContains(x.Name);
				}).ToArray();
				foreach (var command in commands)
				{
					command.ToggleEnabled();
				}
				var text = commands.Any() ? String.Join("`, `", commands.Select(x => x.Name)) : "None";
				await MessageUtils.SendMessageAsync(Context.Channel, $"Successfully enabled the following commands: `{text}`.").CAF();
			}
		}
		[Group(nameof(Disable)), ShortAlias(nameof(Disable))]
		public sealed class Disable : SavingModuleBase
		{
			[Command(nameof(All)), ShortAlias(nameof(All)), Priority(1)]
			public async Task All()
			{
				//Only grab commands that are already enabled and are able to be changed.
				var commands = Context.GuildSettings.CommandSwitches.Where(x =>
				{
					return x.Value && !_CommandsUnableToBeTurnedOff.CaseInsContains(x.Name);
				}).ToArray();
				foreach (var command in commands)
				{
					command.ToggleEnabled();
				}
				var text = commands.Any() ? String.Join("`, `", commands.Select(x => x.Name)) : "None";
				await MessageUtils.SendMessageAsync(Context.Channel, $"Successfully disabled the following commands: `{text}`.").CAF();
			}
			[Command]
			public async Task Command(CommandSwitch command)
			{
				if (!command.Value)
				{
					await MessageUtils.SendErrorMessageAsync(Context, new ErrorReason("This command is already disabled.")).CAF();
					return;
				}
				else if (_CommandsUnableToBeTurnedOff.CaseInsContains(command.Name))
				{
					await MessageUtils.SendErrorMessageAsync(Context, new ErrorReason("Please don't try to edit that command.")).CAF();
					return;
				}

				command.ToggleEnabled();
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully disabled `{command.Name}`.").CAF();
			}
			[Command]
			public async Task Command(CommandCategory category)
			{
				//Only grab commands that are already enabled and in the same category and are able to be changed.
				var commands = Context.GuildSettings.GetCommands(category).Where(x =>
				{
					return x.Value && !_CommandsUnableToBeTurnedOff.CaseInsContains(x.Name);
				}).ToArray();
				foreach (var command in commands)
				{
					command.ToggleEnabled();
				}
				var text = commands.Any() ? String.Join("`, `", commands.Select(x => x.Name)) : "None";
				await MessageUtils.SendMessageAsync(Context.Channel, $"Successfully disabled the following commands: `{text}`.").CAF();
			}
		}
	}

	[Group(nameof(ModifyIgnoredCommandChannels)), TopLevelShortAlias(typeof(ModifyIgnoredCommandChannels))]
	[Summary("The bot will ignore commands said on these channels. " +
		"If a command is input then the bot will instead ignore only that command on the given channel.")]
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
					var error = new ErrorReason($"`{channel.FormatChannel()}` is already ignoring commands.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				Context.GuildSettings.IgnoredCommandChannels.Add(channel.Id);
				var resp = $"Successfully added `{channel.FormatChannel()}` to the ignored command channels list.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command]
			public async Task Command([VerifyObject(true, ObjectVerification.CanBeRead, ObjectVerification.CanBeEdited)] ITextChannel channel, CommandSwitch command)
			{
				if (Context.GuildSettings.CommandsDisabledOnChannel.Any(x => x.Id == channel.Id && x.Name.CaseInsEquals(command.Name)))
				{
					var error = new ErrorReason($"`{command.Name}` is already ignored on `{channel.FormatChannel()}`.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				Context.GuildSettings.CommandsDisabledOnChannel.Add(new CommandOverride(command.Name, channel.Id));
				var resp = $"Successfully started ignoring the command `{command.Name}` on `{channel.FormatChannel()}`.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
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
				var resp = $"Successfully started ignoring the following commands on `{channel.FormatChannel()}`: `{cmdNames}`.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
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
					var error = new ErrorReason($"`{channel.FormatChannel()}` is already allowing commands.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				Context.GuildSettings.IgnoredCommandChannels.RemoveAll(x => x == channel.Id);
				var resp = $"Successfully removed `{channel.FormatChannel()}` from the ignored command channels list.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command]
			public async Task Command([VerifyObject(true, ObjectVerification.CanBeRead, ObjectVerification.CanBeEdited)] ITextChannel channel, CommandSwitch command)
			{
				var cmd = Context.GuildSettings.CommandsDisabledOnChannel.SingleOrDefault(x => x.Id == channel.Id && x.Name.CaseInsEquals(command.Name));
				if (cmd == null)
				{
					var error = new ErrorReason($"`{command.Name}` is already unignored on `{channel.FormatChannel()}`.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				var resp = $"Successfully stopped ignoring the command `{command.Name}` on `{channel.FormatChannel()}`.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
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
				var resp = $"Successfully stopped ignoring the following commands on `{channel.FormatChannel()}`: `{cmdNames}`.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
		}
	}

	[Group(nameof(ModifyBotUsers)), TopLevelShortAlias(typeof(ModifyBotUsers))]
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
				await MessageUtils.SendEmbedMessageAsync(Context.Channel, new EmbedWrapper("Bot Permission Types", desc)).CAF();
			}
			[Command]
			public async Task Command(IUser user)
			{
				var botUser = Context.GuildSettings.BotUsers.SingleOrDefault(x => x.UserId == user.Id);
				if (botUser == null || botUser.Permissions == 0)
				{
					var error = new ErrorReason("That user has no extra permissions from the bot.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				var desc = $"`{String.Join("`, `", GuildPerms.ConvertValueToNames(botUser.Permissions))}`";
				var embed = new EmbedWrapper($"Permissions for {user.FormatUser()}", desc);
				await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
			}
		}
		[Command(nameof(Add)), ShortAlias(nameof(Add))]
		public async Task Add(IUser user, [Remainder, OverrideTypeReader(typeof(GuildPermissionsTypeReader))] ulong permissions)
		{
			var botUser = Context.GuildSettings.BotUsers.SingleOrDefault(x => x.UserId == user.Id);
			if (botUser == null)
			{
				Context.GuildSettings.BotUsers.Add(botUser = new BotImplementedPermissions(user.Id, permissions));
			}

			permissions |= (Context.User as IGuildUser).GuildPermissions.RawValue;
			botUser.AddPermissions(permissions);

			var givenPerms = String.Join("`, `", GuildPerms.ConvertValueToNames(permissions));
			var resp = $"Successfully gave `{user.FormatUser()}` the following bot permissions: `{givenPerms}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove(IUser user, [Remainder, OverrideTypeReader(typeof(GuildPermissionsTypeReader))] ulong permissions)
		{
			permissions |= (Context.User as IGuildUser).GuildPermissions.RawValue;

			var botUser = Context.GuildSettings.BotUsers.FirstOrDefault(x => x.UserId == user.Id);
			if (botUser == null)
			{
				var error = new ErrorReason($"`{user.FormatUser()}` does not have any bot permissions to remove");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}
			botUser.RemovePermissions(permissions);

			var takenPerms = String.Join("`, `", GuildPerms.ConvertValueToNames(permissions));
			var resp = $"Successfully removed the following bot permissions from `{user.FormatUser()}`: `{takenPerms}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifyPersistentRoles)), TopLevelShortAlias(typeof(ModifyPersistentRoles))]
	[Summary("Gives a user a role that stays even when they leave and rejoin the server. " +
		"Type `" + nameof(ModifyPersistentRoles) + " [Show]` to see the which users have persistent roles set up. " +
		"Type `" + nameof(ModifyPersistentRoles) + " [Show] [User]` to see the persistent roles of that user.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyPersistentRoles : SavingModuleBase
	{
		[Group(nameof(Show)), ShortAlias(nameof(Show))]
		public sealed class Show : AdvobotModuleBase
		{
			[Command]
			public async Task Command()
			{
				var roles = Context.GuildSettings.PersistentRoles;
				if (!roles.Any())
				{
					var error = new ErrorReason("The guild does not have any persistent roles.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				var desc = roles.FormatNumberedList("{0}", x => x.ToString(Context.Guild as SocketGuild));
				await MessageUtils.SendEmbedMessageAsync(Context.Channel, new EmbedWrapper("Persistent Roles", desc)).CAF();
			}
			[Command]
			public async Task Command(IUser user)
			{
				var roles = Context.GuildSettings.PersistentRoles.Where(x => x.UserId == user.Id);
				if (!roles.Any())
				{
					var error = new ErrorReason($"The user `{user.FormatUser()}` does not have any persistent roles.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				var desc = roles.FormatNumberedList("{0}", x => x.ToString(Context.Guild as SocketGuild));
				await MessageUtils.SendEmbedMessageAsync(Context.Channel, new EmbedWrapper("Persistent Roles", desc)).CAF();
			}
		}
		[Group(nameof(Add)), ShortAlias(nameof(Add))]
		public sealed class Add : SavingModuleBase
		{
			[Command, Priority(1)]
			public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited)] IUser user,
				[VerifyObject(false, ObjectVerification.CanBeEdited)] IRole role)
				=> await CommandRunner(user.Id, role).CAF();
			[Command]
			public async Task Command([OverrideTypeReader(typeof(UserIdTypeReader))] ulong userId,
				[VerifyObject(false, ObjectVerification.CanBeEdited)] IRole role)
				=> await CommandRunner(userId, role).CAF();

			private async Task CommandRunner(ulong userId, IRole role)
			{
				var match = Context.GuildSettings.PersistentRoles.SingleOrDefault(x => x.UserId == userId && x.RoleId == role.Id);
				if (match == null)
				{
					var error = new ErrorReason($"A persistent role already exists for the user id {userId} with the role {role.FormatRole()}.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				Context.GuildSettings.PersistentRoles.Add(new PersistentRole(userId, role));
				var resp = $"Successfully added a persistent role for the user id {userId} with the role {role.FormatRole()}.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
		}
		[Group(nameof(Remove)), ShortAlias(nameof(Remove))]
		public sealed class Remove : SavingModuleBase
		{
			[Command, Priority(1)]
			public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited)] IUser user,
				[VerifyObject(false, ObjectVerification.CanBeEdited)] IRole role)
				=> await CommandRunner(user.Id, role).CAF();
			[Command]
			public async Task Command([OverrideTypeReader(typeof(UserIdTypeReader))] ulong userId,
				[VerifyObject(false, ObjectVerification.CanBeEdited)] IRole role)
				=> await CommandRunner(userId, role).CAF();

			private async Task CommandRunner(ulong userId, IRole role)
			{
				var match = Context.GuildSettings.PersistentRoles.SingleOrDefault(x => x.UserId == userId && x.RoleId == role.Id);
				if (match == null)
				{
					var error = new ErrorReason($"No persistent role exists for the user id {userId} with the role {role.FormatRole()}.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				Context.GuildSettings.PersistentRoles.Remove(match);
				var resp = $"Successfully removed the persistent role for the user id {userId} with the role {role.FormatRole()}.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
		}
	}

	[Group(nameof(ModifyChannelSettings)), TopLevelShortAlias(typeof(ModifyChannelSettings))]
	[Summary("Image only works solely on attachments. " +
		"Using the command on an already targetted channel turns it off.")]
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
				var resp = $"Successfully removed the channel `{channel.FormatChannel()}` from the image only list.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			else
			{
				Context.GuildSettings.ImageOnlyChannels.Add(channel.Id);
				var resp = $"Successfully added the channel `{channel.FormatChannel()}` to the image only list.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
		}
	}

	[Group(nameof(ModifyGuildNotifs)), TopLevelShortAlias(typeof(ModifyGuildNotifs))]
	[Summary("The bot send a message to the given channel when the self explantory event happens. " +
		"`" + GuildNotification.USER_MENTION + "` will be replaced with the formatted user. " +
		"`" + GuildNotification.USER_STRING + "` will be replaced with a mention of the joining user.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyGuildNotifs : SavingModuleBase
	{
		[Command(nameof(Welcome)), ShortAlias(nameof(Welcome))]
		public async Task Welcome([VerifyObject(true, ObjectVerification.CanModifyPermissions)] ITextChannel channel, [Remainder] CustomArguments<GuildNotification> arguments)
		{
			Context.GuildSettings.WelcomeMessage = arguments.CreateObject(channel);
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully set the welcome message.").CAF();
		}
		[Command(nameof(Goodbye)), ShortAlias(nameof(Goodbye))]
		public async Task Goodbye([VerifyObject(true, ObjectVerification.CanModifyPermissions)] ITextChannel channel, [Remainder] CustomArguments<GuildNotification> arguments)
		{
			Context.GuildSettings.GoodbyeMessage = arguments.CreateObject(channel);
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully set the goodbye message.").CAF();
		}
	}

	[Group(nameof(TestGuildNotifs)), TopLevelShortAlias(typeof(TestGuildNotifs))]
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
				await MessageUtils.SendErrorMessageAsync(Context, new ErrorReason("The welcome notification does not exist.")).CAF();
				return;
			}

			await notif.SendAsync(null).CAF();
		}
		[Command(nameof(Goodbye)), ShortAlias(nameof(Goodbye))]
		public async Task Goodbye()
		{
			var notif = Context.GuildSettings.GoodbyeMessage;
			if (notif == null)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new ErrorReason("The goodbye notification does not exist.")).CAF();
				return;
			}

			await notif.SendAsync(null).CAF();
		}
	}

	[Group(nameof(DisplayGuildSettings)), TopLevelShortAlias(typeof(DisplayGuildSettings))]
	[Summary("Displays guild settings.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(true)]
	public sealed class DisplayGuildSettings : AdvobotModuleBase
	{
		[Command(nameof(Show)), ShortAlias(nameof(Show)), Priority(1)]
		public async Task Show()
		{
			var desc = $"`{String.Join("`, `", Core.Classes.Settings.GuildSettings.GetSettings().Select(x => x.Name))}`";
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, new EmbedWrapper("Setting Names", desc)).CAF();
		}
		[Command(nameof(All)), ShortAlias(nameof(All)), Priority(1)]
		public async Task All()
		{
			var text = Context.GuildSettings.ToString();
			await MessageUtils.SendTextFileAsync(Context.Channel, text, "Guild_Settings", "Guild Settings").CAF();
		}
		[Command]
		public async Task Command([OverrideTypeReader(typeof(SettingTypeReader.GuildSettingTypeReader))] PropertyInfo settingName)
		{
			var desc = Context.GuildSettings.Format(settingName);
			if (desc.Length <= Constants.MAX_DESCRIPTION_LENGTH)
			{
				await MessageUtils.SendEmbedMessageAsync(Context.Channel, new EmbedWrapper(settingName.Name, desc)).CAF();
			}
			else
			{
				await MessageUtils.SendTextFileAsync(Context.Channel, desc, settingName.Name, settingName.Name).CAF();
			}
		}
	}

	[Group(nameof(GetFile)), TopLevelShortAlias(typeof(GetFile))]
	[Summary("Sends the file containing all the guild's settings.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class GetFile : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			var file = IOUtils.GetServerDirectoryFile(Context.Guild.Id, Constants.GUILD_SETTINGS_LOCATION);
			if (!file.Exists)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new ErrorReason("The guild settings file does not exist.")).CAF();
				return;
			}
			await Context.Channel.SendFileAsync(file.FullName).CAF();
		}
	}
}
