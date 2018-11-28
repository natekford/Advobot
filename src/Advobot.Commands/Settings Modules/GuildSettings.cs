using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Users;
using Advobot.Classes.Attributes.ParameterPreconditions.StringValidation;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.Modules;
using Advobot.Classes.Settings;
using Advobot.Classes.TypeReaders;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using CPerm = Discord.ChannelPermission;

namespace Advobot.Commands
{
	public sealed class GuildSettings : ModuleBase
	{
		[Group(nameof(ShowGuildSettings)), ModuleInitialismAlias(typeof(ShowGuildSettings))]
		[Summary("Shows information about guild settings.")]
		[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
		[EnabledByDefault(true)]
		public sealed class ShowGuildSettings : AdvobotSettingsModuleBase<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.GuildSettings;

			[ImplicitCommand, ImplicitAlias, Priority(1)]
			public async Task GetFile()
				=> await GetFileAsync(BotSettings).CAF();
			[ImplicitCommand, ImplicitAlias, Priority(1)]
			public async Task Names()
				=> await ShowNamesAsync().CAF();
			[ImplicitCommand, ImplicitAlias, Priority(1)]
			public async Task All()
				=> await ShowAllAsync().CAF();
			[Command]
			public async Task Command(string settingName)
				=> await ShowAsync(settingName).CAF();
			[Command]
			public async Task Command(string settingName, IUser user)
				=> await ShowUserAsync(settingName, user).CAF();
		}

		[Group(nameof(ModifyGuildSettings)), ModuleInitialismAlias(typeof(ModifyGuildSettings))]
		[Summary("Modify the given setting on the guild.")]
		[UserPermissionRequirement(GuildPermission.Administrator)]
		[EnabledByDefault(false)]
		public sealed class ModifyGuildSettings : AdvobotSettingsSavingModuleBase<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.GuildSettings;

			[ImplicitCommand, ImplicitAlias]
			public Task Reset(string settingName)
				=> ResetAsync(settingName);
			[ImplicitCommand, ImplicitAlias]
			public Task Prefix([ValidatePrefix] string value)
				=> ModifyAsync(x => x.Prefix, value);
			[ImplicitCommand, ImplicitAlias]
			public Task NonVerboseErrors(AddBoolean value)
				=> ModifyAsync(x => x.NonVerboseErrors, value);
			//TODO: rewrite the log channel stuff? or not cause the user has to be admin to execute this meaning they can see every channel
			//TODO: validate invoker has higher role than bot
			[ImplicitCommand, ImplicitAlias]
			public Task ServerLogId([Optional, ValidateTextChannel(CPerm.ManageChannels)] SocketTextChannel value)
				=> ModifyAsync(x => x.ServerLogId, value?.Id ?? 0);
			[ImplicitCommand, ImplicitAlias]
			public Task ModLogId([Optional, ValidateTextChannel(CPerm.ManageChannels)] SocketTextChannel value)
				=> ModifyAsync(x => x.ModLogId, value?.Id ?? 0);
			[ImplicitCommand, ImplicitAlias]
			public Task ImageLogId([Optional, ValidateTextChannel(CPerm.ManageChannels)] SocketTextChannel value)
				=> ModifyAsync(x => x.ImageLogId, value?.Id ?? 0);
			[ImplicitCommand, ImplicitAlias]
			public Task MuteRoleId([NotEveryoneOrManaged] SocketRole value)
				=> ModifyAsync(x => x.MuteRoleId, value.Id);
			[ImplicitCommand, ImplicitAlias]
			public Task LogActions(AddBoolean add, params LogAction[] values)
				=> ModifyCollectionAsync(x => x.LogActions, add, values);
			[ImplicitCommand, ImplicitAlias]
			public Task ImageOnlyChannels(
				AddBoolean add,
				[ValidateTextChannel(CPerm.ManageChannels)] params SocketTextChannel[] values)
				=> ModifyCollectionAsync(x => x.ImageOnlyChannels, add, values.Select(x => x.Id));
			[ImplicitCommand, ImplicitAlias]
			public Task IgnoredLogChannels(
				AddBoolean add,
				[ValidateTextChannel(CPerm.ManageChannels)] params SocketTextChannel[] values)
				=> ModifyCollectionAsync(x => x.IgnoredLogChannels, add, values.Select(x => x.Id));
			[ImplicitCommand, ImplicitAlias]
			public Task IgnoredXpChannels(
				AddBoolean add,
				[ValidateTextChannel(CPerm.ManageChannels)] params SocketTextChannel[] values)
				=> ModifyCollectionAsync(x => x.IgnoredXpChannels, add, values.Select(x => x.Id));
			[ImplicitCommand, ImplicitAlias]
			public Task IgnoredCommandChannels(
				AddBoolean add,
				[ValidateTextChannel(CPerm.ManageChannels)] params SocketTextChannel[] values)
				=> ModifyCollectionAsync(x => x.IgnoredCommandChannels, add, values.Select(x => x.Id));

			[ImplicitCommand, ImplicitAlias]
			public Task Quotes(AddBoolean add, string name, [Optional, Remainder] string text)
				=> ModifyCollectionAsync(x => x.Quotes, add, new[] { new Quote(name, text ?? "") });


			//TODO: go back to old way in separate command because this is kind of unwieldy?
			[ImplicitCommand, ImplicitAlias]
			public Task BotUsers(
				AddBoolean add,
				IUser user,
				[Remainder, OverrideTypeReader(typeof(PermissionsTypeReader<GuildPermission>))] ulong permissions)
				=> ModifyCollectionValuesAsync(
					x => x.BotUsers,
					x => x.UserId == user.Id,
					() => new BotUser(user.Id),
					x =>
					{
						var modified = x.ModifyPermissions(add, Context.User, permissions);
						return $"Successfully {(add ? "removed" : "added")} the following bot permissions on `{user.Format()}`: `{modified}`.";
					});

			[ImplicitCommand, ImplicitAlias]
			public Task WelcomeMessage(
				[ValidateTextChannel(CPerm.ManageChannels, FromContext = true)] SocketTextChannel channel,
				[Remainder] GuildNotification args)
			{
				args.ChannelId = channel.Id;
				return ModifyAsync(x => x.WelcomeMessage, args);
			}
			[ImplicitCommand, ImplicitAlias]
			public Task GoodbyeMessage(
				[ValidateTextChannel(CPerm.ManageChannels, FromContext = true)] SocketTextChannel channel,
				[Remainder] GuildNotification args)
			{
				args.ChannelId = channel.Id;
				return ModifyAsync(x => x.GoodbyeMessage, args);
			}
		}

		[Group(nameof(ModifyCommands)), ModuleInitialismAlias(typeof(ModifyCommands))]
		[Summary("Turns a command on or off. " +
			"Can turn all commands in a category on or off too. " +
			"Cannot turn off commands which are untoggleable.")]
		[UserPermissionRequirement(GuildPermission.Administrator)]
		[EnabledByDefault(true, AbleToToggle = false)]
		public sealed class ModifyCommands : AdvobotSettingsSavingModuleBase<IGuildSettings>
		{
			public IHelpEntryService HelpEntries { get; set; }

			protected override IGuildSettings Settings => Context.GuildSettings;

			[ImplicitCommand, ImplicitAlias, Priority(1)]
			public async Task All(AddBoolean enable)
			{
				var values = HelpEntries.Select(x => new ValueToModify(x, enable));
				var commands = Context.GuildSettings.CommandSettings.ModifyCommandValues(values);
				var text = commands.Any() ? string.Join("`, `", commands) : "None";
				await ReplyAsync($"Successfully {GetAction(enable)} the following commands: `{text}`.").CAF();
			}
			[ImplicitCommand, ImplicitAlias, Priority(1)]
			public async Task Category(AddBoolean enable, string category)
			{
				if (!HelpEntries.GetCategories().CaseInsContains(category))
				{
					await ReplyErrorAsync($"`{category}` is not a valid category.").CAF();
					return;
				}
				//Only grab commands that are already disabled and in the same category and are able to be changed.
				var values = HelpEntries.GetHelpEntries(category).Select(x => new ValueToModify(x, enable));
				var commands = Context.GuildSettings.CommandSettings.ModifyCommandValues(values);
				var text = commands.Any() ? string.Join("`, `", commands) : "None";
				await ReplyAsync($"Successfully {GetAction(enable)} the following commands: `{text}`.").CAF();
			}
			[Command]
			public async Task Command(AddBoolean enable, IHelpEntry command)
			{
				if (!command.AbleToBeToggled)
				{
					await ReplyErrorAsync($"{command.Name} cannot be edited.").CAF();
					return;
				}
				if (!Context.GuildSettings.CommandSettings.ModifyCommandValue(new ValueToModify(command, enable)))
				{
					await ReplyErrorAsync($"{command.Name} is already enabled.").CAF();
					return;
				}
				await ReplyTimedAsync($"Successfully {GetAction(enable)} `{command.Name}`.").CAF();
			}

			private string GetAction(bool value) => value ? "enabled" : "disabled";
		}

		[Group(nameof(ModifyIgnoredCommandChannels)), ModuleInitialismAlias(typeof(ModifyIgnoredCommandChannels))]
		[Summary("The bot will ignore commands said on these channels. " +
			"If a command is input then the bot will instead ignore only that command on the given channel.")]
		[UserPermissionRequirement(GuildPermission.Administrator)]
		[EnabledByDefault(true, AbleToToggle = false)]
		public sealed class ModifyIgnoredCommandChannels : AdvobotSettingsSavingModuleBase<IGuildSettings>
		{
			public IHelpEntryService HelpEntries { get; set; }

			protected override IGuildSettings Settings => Context.GuildSettings;

			/*
			[Command]
			public async Task Command(
				AddBoolean enable,
				[ValidateObject(Verif.CanBeViewed, Verif.CanBeEdited, IfNullCheckFromContext = true)] ITextChannel channel)
				=> await ModifyCollectionAsync(x => x.IgnoredCommandChannels, enable, channel.Id).CAF();*/
			[ImplicitCommand, ImplicitAlias, Priority(1)]
			public async Task Category(
				AddBoolean enable,
				string category,
				[ValidateTextChannel(FromContext = true)] SocketTextChannel channel)
			{
				if (!HelpEntries.GetCategories().CaseInsContains(category))
				{
					await ReplyErrorAsync($"`{category}` is not a valid category.").CAF();
					return;
				}
				var values = HelpEntries.GetHelpEntries(category).Select(x => new ValueToModify(x, enable));
				var commands = Context.GuildSettings.CommandSettings.ModifyOverrides(values, channel);
				var resp = $"Successfully {GetAction(enable)} ignoring the following commands on `{channel.Format()}`: `{commands.Join("`, `")}`.";
				await ReplyTimedAsync(resp).CAF();
			}
			[Command]
			public async Task Command(
				AddBoolean enable,
				IHelpEntry helpEntry,
				[ValidateTextChannel(FromContext = true)] SocketTextChannel channel)
			{
				if (!Context.GuildSettings.CommandSettings.ModifyOverride(new ValueToModify(helpEntry, true), channel))
				{
					await ReplyErrorAsync($"`{helpEntry.Name}` is already {GetAction(enable)} on `{channel.Format()}`.").CAF();
					return;
				}
				await ReplyTimedAsync($"Successfully {GetAction(enable)} the command `{helpEntry.Name}` on `{channel.Format()}`.").CAF();
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
						var error = $"`{channel.Format()}` is already ignoring commands.");
						await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
						return;
					}

					Context.GuildSettings.IgnoredCommandChannels.Add(channel.Id);
					var resp = $"Successfully added `{channel.Format()}` to the ignored command channels list.";
					await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
				}
				[ImplicitCommand, Priority(1)]
				[RequireServices(typeof(IHelpEntryService))]
				public async Task Category(
					[ValidateObject(Verif.CanBeViewed, Verif.CanBeEdited, IfNullCheckFromContext = true)] ITextChannel channel,
					string category)
				{
					var helpEntries = Context.Provider.GetRequiredService<IHelpEntryService>();
					if (!helpEntries.GetCategories().CaseInsContains(category))
					{
						await MessageUtils.SendErrorMessageAsync(Context, $"`{category}` is not a valid category.")).CAF();
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
						var error = $"`{helpEntry.Name}` is already ignored on `{channel.Format()}`.");
						await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
						return;
					}
					var resp = $"Successfully started ignoring the command `{helpEntry.Name}` on `{channel.Format()}`.";
					await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
				}
			}
			 */

			private string GetAction(bool value) => value ? "unignored" : "ignored";
		}

		/*
		[Category(typeof(ModifyBotUsers)), Group(nameof(ModifyBotUsers)), TopLevelShortAlias(typeof(ModifyBotUsers))]
		[Summary("Gives a user permissions in the bot but not on Discord itself. " +
			"Type `" + nameof(ModifyBotUsers) + " [" + nameof(Show) + "]` to see the available permissions. " +
			"Type `" + nameof(ModifyBotUsers) + " [" + nameof(Show) + "] [User]` to see the permissions of that user.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public sealed class ModifyBotUsers : AdvobotSettingsSavingModuleBase<IGuildSettings>
		{
			[ImplicitCommand]
			public async Task Show(IUser user)
			{
				var botUser = Context.GuildSettings.BotUsers.SingleOrDefault(x => x.UserId == user.Id);
				if (botUser == null || botUser.Permissions == 0)
				{
					var error = $"`{user.Format()}` has no extra permissions from the bot.");
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
			[Command]
			public async Task Command(AddBoolean add, IUser user, [Remainder, OverrideTypeReader(typeof(GuildPermissionsTypeReader))] ulong permissions)
			{
				var botUser = Context.GuildSettings.BotUsers.SingleOrDefault(x => x.UserId == user.Id);
				if (add && botUser == null)
				{
					Context.GuildSettings.BotUsers.Add(botUser = new BotUser(user.Id, permissions));
				}
				if (!add && botUser == null)
				{
					var error = $"`{user.Format()}` does not have any bot permissions to remove");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				var modifiedPerms = string.Join("`, `", botUser.ModifyPermissions(add, (IGuildUser)Context.User, permissions));
				var resp = $"Successfully {(add ? "removed" : "added")} the following bot permissions on `{user.Format()}`: `{modifiedPerms}`.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}

			protected override IGuildSettings GetSettings() => Context.GuildSettings;
		}*/

		[Group(nameof(ModifyPersistentRoles)), ModuleInitialismAlias(typeof(ModifyPersistentRoles))]
		[Summary("Gives a user a role that stays even when they leave and rejoin the server.")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		//[SaveGuildSettings]
		public sealed class ModifyPersistentRoles : AdvobotModuleBase
		{
			[Command, Priority(1)]
			public async Task Command(AddBoolean add, [ValidateUser] SocketUser user, [ValidateRole] SocketRole role)
				=> await CommandRunner(add, user.Id, role).CAF();
			[Command] //Should go into the above one if a valid user, so should be fine to not check this one for permission
			public async Task Command(AddBoolean add, ulong userId, [ValidateRole] SocketRole role)
				=> await CommandRunner(add, userId, role).CAF();

			//TODO: rewrite
			private async Task CommandRunner(bool add, ulong userId, SocketRole role)
			{
				if (Context.GuildSettings.PersistentRoles.TryGetSingle(x => x.UserId == userId && x.RoleId == role.Id, out var match) == add)
				{
					var start = add ? "A" : "No";
					await ReplyErrorAsync($"{start} persistent role exists for the user id `{userId}` with the role `{role.Format()}`.").CAF();
					return;
				}

				if (add)
				{
					Context.GuildSettings.PersistentRoles.Add(new PersistentRole(userId, role));
				}
				else
				{
					Context.GuildSettings.PersistentRoles.Remove(match);
				}

				var resp = add ? "added a" : "removed the";
				await ReplyTimedAsync($"Successfully {resp} persistent role for the user id `{userId}` with the role `{role.Format()}`.").CAF();
			}
		}

		/* Implemented by editing the image only list
		[Category(typeof(ModifyChannelSettings)), Group(nameof(ModifyChannelSettings)), TopLevelShortAlias(typeof(ModifyChannelSettings))]
		[Summary("Image only works solely on attachments. " +
			"Using the command on an already targetted channel turns it off.")]
		[UserPermissionRequirement(GuildPermission.ManageChannels }, null)]
		[DefaultEnabled(false)]
		[SaveGuildSettings]
		public sealed class ModifyChannelSettings : AdvobotModuleBase
		{
			[ImplicitCommand]
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
		}*/

		/*
		[Category(typeof(ModifyGuildNotifs)), Group(nameof(ModifyGuildNotifs)), TopLevelShortAlias(typeof(ModifyGuildNotifs))]
		[Summary("The bot send a message to the given channel when the self explantory event happens. " +
			"`" + GuildNotification.USER_MENTION + "` will be replaced with the formatted user. " +
			"`" + GuildNotification.USER_STRING + "` will be replaced with a mention of the joining user.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		[SaveGuildSettings]
		public sealed class ModifyGuildNotifs : AdvobotModuleBase
		{
			[ImplicitCommand]
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
			[ImplicitCommand]
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
		}*/

		[Group(nameof(TestGuildNotifs)), ModuleInitialismAlias(typeof(TestGuildNotifs))]
		[Summary("Sends the given guild notification in order to test it.")]
		[UserPermissionRequirement(GuildPermission.Administrator)]
		[EnabledByDefault(false)]
		public sealed class TestGuildNotifs : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias]
			public Task Welcome()
				=> CommandRunner(Context.GuildSettings.WelcomeMessage);
			[ImplicitCommand, ImplicitAlias]
			public Task Goodbye()
				=> CommandRunner(Context.GuildSettings.GoodbyeMessage);

			private Task CommandRunner(GuildNotification notification, [CallerMemberName] string caller = null)
			{
				return notification != null
					? notification.SendAsync(Context.Guild, null)
					: ReplyErrorAsync($"The `{caller}` notification does not exist.");
			}
		}
	}
}
