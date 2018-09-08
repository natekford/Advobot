using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Settings;
using Advobot.Classes.TypeReaders;
using Advobot.Commands.Misc;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Commands.GuildSettings
{
	[Category(typeof(ShowGuildSettings)), Group(nameof(ShowGuildSettings)), TopLevelShortAlias(typeof(ShowGuildSettings))]
	[Summary("Shows information about guild settings.")]
	[PermissionRequirement(new[] { PermissionRequirementAttribute.GenericPerms }, null)]
	[DefaultEnabled(true)]
	public sealed class ShowGuildSettings : AdvobotSettingsModuleBase<IGuildSettings>
	{
		[Command(nameof(GetFileAsync)), ShortAlias(nameof(GetFileAsync)), Priority(1)]
		public async Task GetFile()
			=> await GetFileAsync(Context.BotSettings).CAF();
		[Command(nameof(Names)), ShortAlias(nameof(Names)), Priority(1)]
		public async Task Names()
			=> await ShowNamesAsync().CAF();
		[Command(nameof(All)), ShortAlias(nameof(All)), Priority(1)]
		public async Task All()
			=> await ShowAllAsync().CAF();
		[Command]
		public async Task Command(string settingName)
			=> await ShowAsync(settingName).CAF();

		protected override IGuildSettings GetSettings() => Context.GuildSettings;
	}

	public class NameableEqualityComparer : IEqualityComparer<INameable>
	{
		public bool Equals(INameable x, INameable y) => x?.Name == y?.Name;
		public int GetHashCode(INameable obj) => obj.GetHashCode();
	}

	[Category(typeof(ModifyGuildSettings)), Group(nameof(ModifyGuildSettings)), TopLevelShortAlias(typeof(ModifyGuildSettings))]
	[Summary("Modify the given setting on the guild.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyGuildSettings : AdvobotSettingsSavingModuleBase<IGuildSettings>
	{
		static ModifyGuildSettings()
		{
			RegisterEqualityComparer<Quote>(new NameableEqualityComparer());
		}

		[Command(nameof(Reset)), ShortAlias(nameof(Reset))]
		public async Task Reset(string settingName)
			=> await ResetAsync(settingName).CAF();
		[Command(nameof(IGuildSettings.Prefix)), ShortAlias(nameof(IGuildSettings.Prefix))]
		public async Task Prefix([ValidateString(Target.Prefix)] string value)
			=> await ModifyAsync(x => x.Prefix, value).CAF();
		[Command(nameof(IGuildSettings.NonVerboseErrors)), ShortAlias(nameof(IGuildSettings.NonVerboseErrors))]
		public async Task NonVerboseErrors(AddBoolean value)
			=> await ModifyAsync(x => x.NonVerboseErrors, value).CAF();
		//TODO: rewrite the log channel stuff? or not cause the user has to be admin to execute this meaning they can see every channel
		//TODO: validate invoker has higher role than bot
		[Command(nameof(IGuildSettings.ServerLogId)), ShortAlias(nameof(IGuildSettings.ServerLogId))]
		public async Task ServerLogId([Optional, ValidateObject(Verif.CanBeViewed, Verif.CanModifyPermissions)] SocketTextChannel value)
			=> await ModifyAsync(x => x.ServerLogId, value?.Id ?? 0).CAF();
		[Command(nameof(IGuildSettings.ModLogId)), ShortAlias(nameof(IGuildSettings.ModLogId))]
		public async Task ModLogId([Optional, ValidateObject(Verif.CanBeViewed, Verif.CanModifyPermissions)] SocketTextChannel value)
			=> await ModifyAsync(x => x.ModLogId, value?.Id ?? 0).CAF();
		[Command(nameof(IGuildSettings.ImageLogId)), ShortAlias(nameof(IGuildSettings.ImageLogId))]
		public async Task ImageLogId([Optional, ValidateObject(Verif.CanBeViewed, Verif.CanModifyPermissions)] SocketTextChannel value)
			=> await ModifyAsync(x => x.ImageLogId, value?.Id ?? 0).CAF();
		[Command(nameof(IGuildSettings.MuteRoleId)), ShortAlias(nameof(IGuildSettings.MuteRoleId))]
		public async Task MuteRoleId([ValidateObject(Verif.IsNotEveryone, Verif.IsNotManaged, Verif.CanBeEdited)] SocketRole value)
			=> await ModifyAsync(x => x.MuteRoleId, value.Id).CAF();
		[Command(nameof(IGuildSettings.LogActions)), ShortAlias(nameof(IGuildSettings.LogActions))]
		public async Task LogActions(AddBoolean add, params LogAction[] values)
			=> await ModifyCollectionAsync(x => x.LogActions, add, values).CAF();
		[Command(nameof(IGuildSettings.ImageOnlyChannels)), ShortAlias(nameof(IGuildSettings.ImageOnlyChannels))]
		public async Task ImageOnlyChannels(
			AddBoolean add,
			[ValidateObject(Verif.CanBeViewed, Verif.CanModifyPermissions)] params SocketTextChannel[] values)
			=> await ModifyCollectionAsync(x => x.ImageOnlyChannels, add, values.Select(x => x.Id)).CAF();
		[Command(nameof(IGuildSettings.IgnoredLogChannels)), ShortAlias(nameof(IGuildSettings.IgnoredLogChannels))]
		public async Task IgnoredLogChannels(
			AddBoolean add,
			[ValidateObject(Verif.CanBeViewed, Verif.CanModifyPermissions)] params SocketTextChannel[] values)
			=> await ModifyCollectionAsync(x => x.IgnoredLogChannels, add, values.Select(x => x.Id)).CAF();
		[Command(nameof(IGuildSettings.IgnoredXpChannels)), ShortAlias(nameof(IGuildSettings.IgnoredXpChannels))]
		public async Task IgnoredXpChannels(
			AddBoolean add,
			[ValidateObject(Verif.CanBeViewed, Verif.CanModifyPermissions)] params SocketTextChannel[] values)
			=> await ModifyCollectionAsync(x => x.IgnoredXpChannels, add, values.Select(x => x.Id)).CAF();
		[Command(nameof(IGuildSettings.IgnoredCommandChannels)), ShortAlias(nameof(IGuildSettings.IgnoredCommandChannels))]
		public async Task IgnoredCommandChannels(
			AddBoolean add,
			[ValidateObject(Verif.CanBeViewed, Verif.CanModifyPermissions)] params SocketTextChannel[] values)
			=> await ModifyCollectionAsync(x => x.IgnoredCommandChannels, add, values.Select(x => x.Id)).CAF();

		[Command(nameof(IGuildSettings.Quotes)), ShortAlias(nameof(IGuildSettings.Quotes))]
		public async Task Quotes(AddBoolean add, string name, [Optional, Remainder] string text)
			=> await ModifyCollectionAsync(x => x.Quotes, add, new Quote(name, text ?? "")).CAF();
		
		protected override IGuildSettings GetSettings() => Context.GuildSettings;
	}

	[Category(typeof(ModifyCommands)), Group(nameof(ModifyCommands)), TopLevelShortAlias(typeof(ModifyCommands))]
	[Summary("Turns a command on or off. " +
		"Can turn all commands in a category on or off too. " +
		"Cannot turn off commands which are untoggleable.")]
	[DefaultEnabled(true, AbleToToggle = false)]
	[PermissionRequirement(null, null)]
	[RequireServices(typeof(IHelpEntryService))]
	public sealed class ModifyCommands : AdvobotSettingsSavingModuleBase<IGuildSettings>
	{
		[Command(nameof(All)), ShortAlias(nameof(All)), Priority(1)]
		public async Task All(AddBoolean enable)
		{
			var helpEntries = Context.Provider.GetRequiredService<IHelpEntryService>();
			var values = helpEntries.Select(x => new ValueToModify(x, enable));
			var commands = Context.GuildSettings.CommandSettings.ModifyCommandValues(values);
			var text = commands.Any() ? string.Join("`, `", commands) : "None";
			await MessageUtils.SendMessageAsync(Context.Channel, $"Successfully {GetAction(enable)} the following commands: `{text}`.").CAF();
		}
		[Command(nameof(Category)), ShortAlias(nameof(Category)), Priority(1)]
		public async Task Category(AddBoolean enable, string category)
		{
			var helpEntries = Context.Provider.GetRequiredService<IHelpEntryService>();
			if (!helpEntries.GetCategories().CaseInsContains(category))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error($"`{category}` is not a valid category.")).CAF();
				return;
			}
			//Only grab commands that are already disabled and in the same category and are able to be changed.
			var values = helpEntries.GetHelpEntries(category).Select(x => new ValueToModify(x, enable));
			var commands = Context.GuildSettings.CommandSettings.ModifyCommandValues(values);
			var text = commands.Any() ? string.Join("`, `", commands.Select(x => x)) : "None";
			await MessageUtils.SendMessageAsync(Context.Channel, $"Successfully {GetAction(enable)} the following commands: `{text}`.").CAF();
		}
		[Command]
		public async Task Command(AddBoolean enable, IHelpEntry command)
		{
			if (!command.AbleToBeToggled)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error($"{command.Name} cannot be edited.")).CAF();
				return;
			}
			if (!Context.GuildSettings.CommandSettings.ModifyCommandValue(new ValueToModify(command, enable)))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error($"{command.Name} is already enabled.")).CAF();
				return;
			}
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully {GetAction(enable)} `{command.Name}`.").CAF();
		}

		private string GetAction(bool value) => value ? "enabled" : "disabled";
		protected override IGuildSettings GetSettings() => Context.GuildSettings;
	}

	[Category(typeof(ModifyIgnoredCommandChannels)), Group(nameof(ModifyIgnoredCommandChannels)), TopLevelShortAlias(typeof(ModifyIgnoredCommandChannels))]
	[Summary("The bot will ignore commands said on these channels. " +
		"If a command is input then the bot will instead ignore only that command on the given channel.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(true, AbleToToggle = false)]
	public sealed class ModifyIgnoredCommandChannels : AdvobotSettingsSavingModuleBase<IGuildSettings>
	{
		/*
		[Command]
		public async Task Command(
			AddBoolean enable,
			[ValidateObject(Verif.CanBeViewed, Verif.CanBeEdited, IfNullCheckFromContext = true)] ITextChannel channel)
			=> await ModifyCollectionAsync(x => x.IgnoredCommandChannels, enable, channel.Id).CAF();*/
		[Command(nameof(Category)), ShortAlias(nameof(Category)), Priority(1)]
		[RequireServices(typeof(IHelpEntryService))]
		public async Task Category(
			AddBoolean enable,
			string category,
			[ValidateObject(Verif.CanBeViewed, Verif.CanBeEdited, IfNullCheckFromContext = true)] ITextChannel channel)
		{
			var helpEntries = Context.Provider.GetRequiredService<IHelpEntryService>();
			if (!helpEntries.GetCategories().CaseInsContains(category))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error($"`{category}` is not a valid category.")).CAF();
				return;
			}
			var values = helpEntries.GetHelpEntries(category).Select(x => new ValueToModify(x, enable));
			var commands = Context.GuildSettings.CommandSettings.ModifyOverrides(values, channel);
			var resp = $"Successfully {GetAction(enable)} ignoring the following commands on `{channel.Format()}`: `{string.Join("`, `", commands)}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command]
		[RequireServices(typeof(IHelpEntryService))]
		public async Task Command(
			AddBoolean enable,
			IHelpEntry helpEntry,
			[ValidateObject(Verif.CanBeViewed, Verif.CanBeEdited, IfNullCheckFromContext = true)] ITextChannel channel)
		{
			if (!Context.GuildSettings.CommandSettings.ModifyOverride(new ValueToModify(helpEntry, true), channel))
			{
				var error = new Error($"`{helpEntry.Name}` is already {GetAction(enable)} on `{channel.Format()}`.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}
			var resp = $"Successfully {GetAction(enable)} the command `{helpEntry.Name}` on `{channel.Format()}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}

		/*
		[Group(nameof(Disable)), ShortAlias(nameof(Disable))]
		public sealed class Disable : AdvobotModuleBase
		{
			[Command]
			public async Task Command(
				[ValidateObject(Verif.CanBeViewed, Verif.CanBeEdited, IfNullCheckFromContext = true)] ITextChannel channel)
			{
				if (Context.GuildSettings.IgnoredCommandChannels.Contains(channel.Id))
				{
					var error = new Error($"`{channel.Format()}` is already ignoring commands.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				Context.GuildSettings.IgnoredCommandChannels.Add(channel.Id);
				var resp = $"Successfully added `{channel.Format()}` to the ignored command channels list.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command(nameof(Category)), ShortAlias(nameof(Category)), Priority(1)]
			[RequireServices(typeof(IHelpEntryService))]
			public async Task Category(
				[ValidateObject(Verif.CanBeViewed, Verif.CanBeEdited, IfNullCheckFromContext = true)] ITextChannel channel,
				string category)
			{
				var helpEntries = Context.Provider.GetRequiredService<IHelpEntryService>();
				if (!helpEntries.GetCategories().CaseInsContains(category))
				{
					await MessageUtils.SendErrorMessageAsync(Context, new Error($"`{category}` is not a valid category.")).CAF();
					return;
				}
				var values = helpEntries.GetHelpEntries(category).Select(x => new ValueToModify(x, false));
				var commands = Context.GuildSettings.CommandSettings.ModifyOverrides(values, channel);
				var resp = $"Successfully started disabled the following commands on `{channel.Format()}`: `{string.Join("`, `", commands)}`.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command]
			[RequireServices(typeof(IHelpEntryService))]
			public async Task Command(
				[ValidateObject(Verif.CanBeViewed, Verif.CanBeEdited, IfNullCheckFromContext = true)] ITextChannel channel,
				IHelpEntry helpEntry)
			{
				if (!Context.GuildSettings.CommandSettings.ModifyOverride(new ValueToModify(helpEntry, false), channel))
				{
					var error = new Error($"`{helpEntry.Name}` is already ignored on `{channel.Format()}`.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}
				var resp = $"Successfully started ignoring the command `{helpEntry.Name}` on `{channel.Format()}`.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
		}
		 */

		private string GetAction(bool value) => value ? "unignored" : "ignored";
		protected override IGuildSettings GetSettings() => Context.GuildSettings;
	}

	[Category(typeof(ModifyBotUsers)), Group(nameof(ModifyBotUsers)), TopLevelShortAlias(typeof(ModifyBotUsers))]
	[Summary("Gives a user permissions in the bot but not on Discord itself. " +
		"Type `" + nameof(ModifyBotUsers) + " [" + nameof(Show) + "]` to see the available permissions. " +
		"Type `" + nameof(ModifyBotUsers) + " [" + nameof(Show) + "] [User]` to see the permissions of that user.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyBotUsers : AdvobotSettingsSavingModuleBase<IGuildSettings>
	{
		[Group(nameof(Show)), ShortAlias(nameof(Show))]
		public sealed class Show : AdvobotModuleBase
		{
			[Command]
			public async Task Command()
			{
				var embed = new EmbedWrapper
				{
					Title = "Bot Permissions",
					Description = $"`{string.Join("`, `", Enum.GetNames(typeof(GuildPermission)))}`"
				};
				await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
			}
			[Command]
			public async Task Command(IUser user)
			{
				var botUser = Context.GuildSettings.BotUsers.SingleOrDefault(x => x.UserId == user.Id);
				if (botUser == null || botUser.Permissions == 0)
				{
					var error = new Error("That user has no extra permissions from the bot.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				var embed = new EmbedWrapper
				{
					Title = $"Permissions for {user.Format()}",
					Description = $"`{string.Join("`, `", EnumUtils.GetFlagNames((GuildPermission)botUser.Permissions))}`"
				};
				await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
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

			permissions |= ((IGuildUser)Context.User).GuildPermissions.RawValue;
			botUser.AddPermissions(permissions);

			var givenPerms = string.Join("`, `", EnumUtils.GetFlagNames((GuildPermission)permissions));
			var resp = $"Successfully gave `{user.Format()}` the following bot permissions: `{givenPerms}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove(IUser user, [Remainder, OverrideTypeReader(typeof(GuildPermissionsTypeReader))] ulong permissions)
		{
			permissions |= ((IGuildUser)Context.User).GuildPermissions.RawValue;

			var botUser = Context.GuildSettings.BotUsers.FirstOrDefault(x => x.UserId == user.Id);
			if (botUser == null)
			{
				var error = new Error($"`{user.Format()}` does not have any bot permissions to remove");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}
			botUser.RemovePermissions(permissions);

			var takenPerms = string.Join("`, `", EnumUtils.GetFlagNames((GuildPermission)permissions));
			var resp = $"Successfully removed the following bot permissions from `{user.Format()}`: `{takenPerms}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}

		protected override IGuildSettings GetSettings() => Context.GuildSettings;
	}

	[Category(typeof(ModifyPersistentRoles)), Group(nameof(ModifyPersistentRoles)), TopLevelShortAlias(typeof(ModifyPersistentRoles))]
	[Summary("Gives a user a role that stays even when they leave and rejoin the server. " +
		"Type `" + nameof(ModifyPersistentRoles) + " [" + nameof(Show) + "]`` to see the which users have persistent roles set up. " +
		"Type `" + nameof(ModifyPersistentRoles) + " [" + nameof(Show) + "]` [User]` to see the persistent roles of that user.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	[SaveGuildSettings]
	public sealed class ModifyPersistentRoles : AdvobotModuleBase
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
					var error = new Error("The guild does not have any persistent roles.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				var embed = new EmbedWrapper
				{
					Title = "Persistent Roles",
					Description = roles.FormatNumberedList(x => x.ToString(Context.Guild as SocketGuild))
				};
				await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
			}
			[Command]
			public async Task Command(IUser user)
			{
				var roles = Context.GuildSettings.PersistentRoles.Where(x => x.UserId == user.Id).ToList();
				if (!roles.Any())
				{
					var error = new Error($"The user `{user.Format()}` does not have any persistent roles.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				var embed = new EmbedWrapper
				{
					Title = "Persistent Roles",
					Description = roles.FormatNumberedList(x => x.ToString(Context.Guild as SocketGuild))
				};
				await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
			}
		}
		[Group(nameof(Add)), ShortAlias(nameof(Add))]
		public sealed class Add : AdvobotModuleBase
		{
			[Command, Priority(1)]
			public async Task Command(
				[ValidateObject(Verif.CanBeEdited)] IUser user,
				[ValidateObject(Verif.CanBeEdited)] IRole role)
				=> await CommandRunner(user.Id, role).CAF();
			[Command] //Should go into the above one if a valid user, so should be fine to not check this one for permission
			public async Task Command(ulong userId, [ValidateObject(Verif.CanBeEdited)] IRole role)
				=> await CommandRunner(userId, role).CAF();

			private async Task CommandRunner(ulong userId, IRole role)
			{
				var match = Context.GuildSettings.PersistentRoles.SingleOrDefault(x => x.UserId == userId && x.RoleId == role.Id);
				if (match != null)
				{
					var error = new Error($"A persistent role already exists for the user id {userId} with the role {role.Format()}.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				Context.GuildSettings.PersistentRoles.Add(new PersistentRole(userId, role));
				var resp = $"Successfully added a persistent role for the user id {userId} with the role {role.Format()}.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
		}
		[Group(nameof(Remove)), ShortAlias(nameof(Remove))]
		public sealed class Remove : AdvobotModuleBase
		{
			[Command, Priority(1)]
			public async Task Command(
				[ValidateObject(Verif.CanBeEdited)] IUser user,
				[ValidateObject(Verif.CanBeEdited)] IRole role)
				=> await CommandRunner(user.Id, role).CAF();
			[Command] //Should go into the above one if a valid user, so should be fine to not check this one for permission
			public async Task Command(ulong userId, [ValidateObject(Verif.CanBeEdited)] IRole role)
				=> await CommandRunner(userId, role).CAF();

			private async Task CommandRunner(ulong userId, IRole role)
			{
				var match = Context.GuildSettings.PersistentRoles.SingleOrDefault(x => x.UserId == userId && x.RoleId == role.Id);
				if (match == null)
				{
					var error = new Error($"No persistent role exists for the user id {userId} with the role {role.Format()}.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				Context.GuildSettings.PersistentRoles.Remove(match);
				var resp = $"Successfully removed the persistent role for the user id {userId} with the role {role.Format()}.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
		}
	}

	[Category(typeof(ModifyChannelSettings)), Group(nameof(ModifyChannelSettings)), TopLevelShortAlias(typeof(ModifyChannelSettings))]
	[Summary("Image only works solely on attachments. " +
		"Using the command on an already targetted channel turns it off.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(false)]
	[SaveGuildSettings]
	public sealed class ModifyChannelSettings : AdvobotModuleBase
	{
		[Command(nameof(ImageOnly)), ShortAlias(nameof(ImageOnly))]
		public async Task ImageOnly([ValidateObject(Verif.CanBeEdited, IfNullCheckFromContext = true)] ITextChannel channel)
		{
			if (Context.GuildSettings.ImageOnlyChannels.Contains(channel.Id))
			{
				Context.GuildSettings.ImageOnlyChannels.Remove(channel.Id);
				var resp = $"Successfully removed the channel `{channel.Format()}` from the image only list.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			else
			{
				Context.GuildSettings.ImageOnlyChannels.Add(channel.Id);
				var resp = $"Successfully added the channel `{channel.Format()}` to the image only list.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
		}
	}

	[Category(typeof(ModifyGuildNotifs)), Group(nameof(ModifyGuildNotifs)), TopLevelShortAlias(typeof(ModifyGuildNotifs))]
	[Summary("The bot send a message to the given channel when the self explantory event happens. " +
		"`" + GuildNotification.USER_MENTION + "` will be replaced with the formatted user. " +
		"`" + GuildNotification.USER_STRING + "` will be replaced with a mention of the joining user.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	[SaveGuildSettings]
	public sealed class ModifyGuildNotifs : AdvobotModuleBase
	{
		[Command(nameof(Welcome)), ShortAlias(nameof(Welcome))]
		public async Task Welcome([ValidateObject(Verif.CanModifyPermissions, IfNullCheckFromContext = true)] ITextChannel channel, [Remainder] NamedArguments<GuildNotification> args)
		{
			if (!args.TryCreateObject(new object[] { channel }, out var obj, out var error))
			{
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}
			Context.GuildSettings.WelcomeMessage = obj;
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully set the welcome message.").CAF();
		}
		[Command(nameof(Goodbye)), ShortAlias(nameof(Goodbye))]
		public async Task Goodbye([ValidateObject(Verif.CanModifyPermissions, IfNullCheckFromContext = true)] ITextChannel channel, [Remainder] NamedArguments<GuildNotification> args)
		{
			if (!args.TryCreateObject(new object[] { channel }, out var obj, out var error))
			{
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}
			Context.GuildSettings.GoodbyeMessage = obj;
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully set the goodbye message.").CAF();
		}
	}

	[Category(typeof(TestGuildNotifs)), Group(nameof(TestGuildNotifs)), TopLevelShortAlias(typeof(TestGuildNotifs))]
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
				await MessageUtils.SendErrorMessageAsync(Context, new Error("The welcome notification does not exist.")).CAF();
				return;
			}

			await notif.SendAsync(Context.Guild as SocketGuild, null).CAF();
		}
		[Command(nameof(Goodbye)), ShortAlias(nameof(Goodbye))]
		public async Task Goodbye()
		{
			var notif = Context.GuildSettings.GoodbyeMessage;
			if (notif == null)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("The goodbye notification does not exist.")).CAF();
				return;
			}

			await notif.SendAsync(Context.Guild as SocketGuild, null).CAF();
		}
	}
}
