using System.Threading.Tasks;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.Modules;
using Advobot.Interfaces;
using Discord;
using Discord.Commands;

namespace Advobot.Commands
{
	public sealed class GuildSettings : ModuleBase
	{
		[Group(nameof(ShowGuildSettings)), ModuleInitialismAlias(typeof(ShowGuildSettings))]
		[Summary("Shows information about guild settings.")]
		[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
		[EnabledByDefault(true)]
		public sealed class ShowGuildSettings : ReadOnlyAdvobotSettingsModuleBase<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.GuildSettings;

			[ImplicitCommand, ImplicitAlias, Priority(1)]
			public Task<RuntimeResult> Json()
				=> Responses.GuildSettings.DisplayJson(Settings);
			[ImplicitCommand, ImplicitAlias, Priority(1)]
			public Task<RuntimeResult> Names()
				=> Responses.GuildSettings.DisplayNames(Settings);
			[ImplicitCommand, ImplicitAlias, Priority(1)]
			public Task<RuntimeResult> All()
				=> Responses.GuildSettings.DisplaySettings(Context.Client, Context.Guild, Settings);
			[Command]
			public Task<RuntimeResult> Command(string name)
				=> Responses.GuildSettings.DisplaySetting(Context.Client, Context.Guild, Settings, name);
		}

		/*
		[Group(nameof(ModifyGuildSettings)), ModuleInitialismAlias(typeof(ModifyGuildSettings))]
		[Summary("Modify the given setting on the guild.")]
		[UserPermissionRequirement(GuildPermission.Administrator)]
		[EnabledByDefault(false)]
		public sealed class ModifyGuildSettings : AdvobotSettingsModuleBase<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.GuildSettings;

			[ImplicitCommand, ImplicitAlias]
			public Task Reset(string settingName)
				=> ResetAsync(settingName);
			[ImplicitCommand, ImplicitAlias]
			public Task Prefix([ValidatePrefix] string value)
				=> ModifyAsync(x => x.Prefix, value);
			[ImplicitCommand, ImplicitAlias]
			public Task NonVerboseErrors(bool value)
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
			public Task LogActions(bool add, params LogAction[] values)
				=> ModifyCollectionAsync(x => x.LogActions, add, values);
			[ImplicitCommand, ImplicitAlias]
			public Task ImageOnlyChannels(
				bool add,
				[ValidateTextChannel(CPerm.ManageChannels)] params SocketTextChannel[] values)
				=> ModifyCollectionAsync(x => x.ImageOnlyChannels, add, values.Select(x => x.Id));
			[ImplicitCommand, ImplicitAlias]
			public Task IgnoredLogChannels(
				bool add,
				[ValidateTextChannel(CPerm.ManageChannels)] params SocketTextChannel[] values)
				=> ModifyCollectionAsync(x => x.IgnoredLogChannels, add, values.Select(x => x.Id));
			[ImplicitCommand, ImplicitAlias]
			public Task IgnoredXpChannels(
				bool add,
				[ValidateTextChannel(CPerm.ManageChannels)] params SocketTextChannel[] values)
				=> ModifyCollectionAsync(x => x.IgnoredXpChannels, add, values.Select(x => x.Id));
			[ImplicitCommand, ImplicitAlias]
			public Task IgnoredCommandChannels(
				bool add,
				[ValidateTextChannel(CPerm.ManageChannels)] params SocketTextChannel[] values)
				=> ModifyCollectionAsync(x => x.IgnoredCommandChannels, add, values.Select(x => x.Id));

			[ImplicitCommand, ImplicitAlias]
			public Task Quotes(bool add, string name, [Optional, Remainder] string text)
				=> ModifyCollectionAsync(x => x.Quotes, add, new[] { new Quote(name, text ?? "") });


			//TODO: go back to old way in separate command because this is kind of unwieldy?
			[ImplicitCommand, ImplicitAlias]
			public Task BotUsers(
				bool add,
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
		}*/

#warning reenable
		/*
		[Group(nameof(ModifyCommands)), ModuleInitialismAlias(typeof(ModifyCommands))]
		[Summary("Turns a command on or off. " +
			"Can turn all commands in a category on or off too. " +
			"Cannot turn off commands which are untoggleable.")]
		[UserPermissionRequirement(GuildPermission.Administrator)]
		[EnabledByDefault(true, AbleToToggle = false)]
		public sealed class ModifyCommands
			: AdvobotSettingsModuleBase<IGuildSettings>, ICollectionSettingModule<string>, ICollectionSettingModule<IHelpEntry>
		{
			public IHelpEntryService HelpEntries { get; set; }

			protected override IGuildSettings Settings => Context.GuildSettings;
			protected override string SettingName => nameof(IGuildSettings.CommandSettings);

			[ImplicitCommand, ImplicitAlias, Priority(1)]
			public Task<RuntimeResult> All(bool enable)
			{
				var commands = Context.GuildSettings.CommandSettings.ModifyCommandValues(HelpEntries, enable);
				return ResponsesOf.ModifyCommands.For(Context).ModifiedMultiple(Subject.Is(commands), enable);
			}
			[ImplicitCommand, ImplicitAlias, Priority(1)]
			public Task<RuntimeResult> Modify(bool enable, string category)
			{
				if (!HelpEntries.GetCategories().CaseInsContains(category))
				{
					return ResponsesOf.ModifyCommands.For(Context).InvalidCategory(Subject.Is(category));
				}
				//Only grab commands that are already disabled and in the same category and are able to be changed.
				var commands = Context.GuildSettings.CommandSettings.ModifyCommandValues(HelpEntries.GetHelpEntries(category), enable);
				return ResponsesOf.ModifyCommands.For(Context).ModifiedMultiple(Subject.Is(commands), enable);
			}
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Modify(bool enable, IHelpEntry command)
			{
				if (!command.AbleToBeToggled)
				{
					return ResponsesOf.ModifyCommands.For(Context).CannotBeEdited(Subject.Is(command.Name));
				}
				if (!Context.GuildSettings.CommandSettings.ModifyCommandValue(command, enable))
				{
					return ResponsesOf.ModifyCommands.For(Context).Unmodified(Subject.Is(command.Name), enable);
				}
				return ResponsesOf.ModifyCommands.For(Context).Modified(Subject.Is(command.Name), enable);
			}
		}*/

		[Group(nameof(ModifyIgnoredCommandChannels)), ModuleInitialismAlias(typeof(ModifyIgnoredCommandChannels))]
		[Summary("The bot will ignore commands said on these channels. " +
			"If a command is input then the bot will instead ignore only that command on the given channel.")]
		[UserPermissionRequirement(GuildPermission.Administrator)]
		[EnabledByDefault(true, AbleToToggle = false)]
		public sealed class ModifyIgnoredCommandChannels : AdvobotSettingsModuleBase<IGuildSettings>
		{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
			public IHelpEntryService HelpEntries { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

			protected override IGuildSettings Settings => Context.GuildSettings;
			//protected override string SettingName => nameof(IGuildSettings.IgnoredCommandChannels);

			/*
			[Command]
			public async Task Command(
				bool enable,
				[ValidateTextChannel(FromContext = true)] SocketTextChannel channel)
				=> await ModifyCollectionAsync(x => x.IgnoredCommandChannels, enable, channel.Id).CAF();*/
				/*
			[ImplicitCommand, ImplicitAlias]
			public Task Category(bool enable, [ValidateCommandCategory] string category, [ValidateTextChannel(FromContext = true)] SocketTextChannel channel)
			{
				throw new NotImplementedException();
				/*
				var commands = Settings.CommandSettings.ModifyOverrides(HelpEntries.GetHelpEntries(category), channel, enable);
				if (!commands.Any())
				{
					return ReplyErrorAsync($"`{category}` is already {(enable ? "unignored" : "ignored")} on `{channel.Format()}`.");
				}
				return ReplyTimedAsync($"Successfully {(enable ? "unignored" : "ignored")} `{commands.Join("`, `")}` on `{channel.Format()}`.");
			}
			[Command]
			public Task Command(bool enable, IHelpEntry helpEntry, [ValidateTextChannel(FromContext = true)] SocketTextChannel channel)
			{
				throw new NotImplementedException();
				/*
				if (!Settings.CommandSettings.ModifyOverride(helpEntry, channel, enable))
				{
					return ReplyErrorAsync($"`{helpEntry.Name}` is already {(enable ? "unignored" : "ignored")} on `{channel.Format()}`.");
				}
				return ReplyTimedAsync($"Successfully {(enable ? "unignored" : "ignored")} `{helpEntry.Name}` on `{channel.Format()}`.");
			}*/
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
			public async Task Command(bool add, IUser user, [Remainder, OverrideTypeReader(typeof(GuildPermissionsTypeReader))] ulong permissions)
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

#warning reenable
		/*
		[Group(nameof(ModifyPersistentRoles)), ModuleInitialismAlias(typeof(ModifyPersistentRoles))]
		[Summary("Gives a user a role that stays even when they leave and rejoin the server.")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class ModifyPersistentRoles : AdvobotSettingsModuleBase<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.GuildSettings;

			[Command, Priority(1)]
			public Task Command(bool add, [ValidateUser] SocketUser user, [ValidateRole] SocketRole role)
				=> CommandRunner(add, user.Id, role);
			[Command] //Should go into the above one if a valid user, so should be fine to not check this one for permission
			public Task Command(bool add, ulong userId, [ValidateRole] SocketRole role)
				=> CommandRunner(add, userId, role);

			private Task CommandRunner(bool add, ulong userId, SocketRole role)
			{
				if (Settings.PersistentRoles.TryGetSingle(x => x.UserId == userId && x.RoleId == role.Id, out var match) == add)
				{
					return ReplyErrorAsync($"{(add ? "A" : "No")} persistent role exists for `{userId}` with `{role.Format()}`.");
				}

				if (add)
				{
					Settings.PersistentRoles.Add(new PersistentRole(userId, role));
				}
				else
				{
					Settings.PersistentRoles.Remove(match);
				}

				var action = add ? "added a" : "removed the";
				return ReplyTimedAsync($"Successfully {action} persistent role for `{userId}` with `{role.Format()}`.");
			}
		}*/

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
			//TODO: make one of these for every event
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Welcome()
				=> Responses.GuildSettings.SendWelcomeNotification(Context.GuildSettings.WelcomeMessage);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Goodbye()
				=> Responses.GuildSettings.SendGoodbyeNotification(Context.GuildSettings.GoodbyeMessage);
		}
	}
}
