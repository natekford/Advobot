using Advobot.Attributes;
using Advobot.Localization;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;
using Advobot.Services.Help;
using Advobot.Settings.Models;

using Discord.Commands;

using static Advobot.Settings.Responses.Settings;

namespace Advobot.Settings.Commands;

[Category(nameof(Settings))]
public sealed class Settings : ModuleBase
{
	[LocalizedGroup(nameof(Groups.ModifyCommands))]
	[LocalizedAlias(nameof(Aliases.ModifyCommands))]
	[LocalizedSummary(nameof(Summaries.ModifyCommands))]
	[Meta("6fb02198-9eab-4e44-a59a-7ba7f7317c10", IsEnabled = true, CanToggle = false)]
	[RequireGuildPermissions]
	public sealed class ModifyCommands : ModuleBase
	{
		[LocalizedGroup(nameof(Groups.Clear))]
		[LocalizedAlias(nameof(Aliases.Clear))]
		[LocalizedSummary(nameof(Summaries.ModifyCommandsClear))]
		public sealed class Clear : ModifyCommandsModuleBase
		{
			public override bool? ShouldEnable => null;

			[Command]
			public Task<RuntimeResult> Command(CommandOverrideEntity entity)
				=> ModifyAll(entity, 0);

			[Command]
			public Task<RuntimeResult> Command(
				CommandOverrideEntity entity,
				params Category[] categories)
				=> ModifyCategories(entity, categories, 0);

			[Command]
			public Task<RuntimeResult> Command(
				CommandOverrideEntity entity,
				params IHelpModule[] commands)
				=> Modify(entity, commands, 0);
		}

		[LocalizedGroup(nameof(Groups.Disable))]
		[LocalizedAlias(nameof(Aliases.Disable))]
		[LocalizedSummary(nameof(Summaries.ModifyCommandsDisable))]
		public sealed class Disable : ModifyCommandsModuleBase
		{
			public override bool? ShouldEnable => false;

			[Command]
			public Task<RuntimeResult> Command(
				int priority,
				CommandOverrideEntity entity)
				=> ModifyAll(entity, priority);

			[Command]
			public Task<RuntimeResult> Command(
				int priority,
				CommandOverrideEntity entity,
				params Category[] categories)
				=> ModifyCategories(entity, categories, priority);

			[Command]
			public Task<RuntimeResult> Command(
				int priority,
				CommandOverrideEntity entity,
				params IHelpModule[] commands)
				=> Modify(entity, commands, priority);
		}

		[LocalizedGroup(nameof(Groups.Enable))]
		[LocalizedAlias(nameof(Aliases.Enable))]
		[LocalizedSummary(nameof(Summaries.ModifyCommandsEnable))]
		public sealed class Enable : ModifyCommandsModuleBase
		{
			public override bool? ShouldEnable => true;

			[Command]
			public Task<RuntimeResult> Command(
				int priority,
				CommandOverrideEntity entity)
				=> ModifyAll(entity, priority);

			[Command]
			public Task<RuntimeResult> Command(
				int priority,
				CommandOverrideEntity entity,
				params Category[] categories)
				=> ModifyCategories(entity, categories, priority);

			[Command]
			public Task<RuntimeResult> Command(
				int priority,
				CommandOverrideEntity entity,
				params IHelpModule[] commands)
				=> Modify(entity, commands, priority);
		}

		public abstract class ModifyCommandsModuleBase : SettingsModuleBase
		{
			public IHelpService HelpEntries { get; set; } = null!;
			public abstract bool? ShouldEnable { get; }

			protected async Task<RuntimeResult> Modify(
				CommandOverrideEntity entity,
				IEnumerable<IHelpModule> commands,
				int priority)
			{
				var overrides = commands.Select(x => new CommandOverride(entity)
				{
					CommandId = x.Id,
					Enabled = ShouldEnable ?? false,
					Priority = priority,
				});

				if (ShouldEnable.HasValue)
				{
					await Db.UpsertCommandOverridesAsync(overrides).ConfigureAwait(false);
					return ModifiedCommands(commands, priority, ShouldEnable.Value);
				}
				else
				{
					await Db.DeleteCommandOverridesAsync(overrides).ConfigureAwait(false);
					return ClearedCommands(commands);
				}
			}

			protected Task<RuntimeResult> ModifyAll(CommandOverrideEntity entity, int priority)
				=> Modify(entity, HelpEntries.GetHelpModules(), priority);

			protected Task<RuntimeResult> ModifyCategories(
				CommandOverrideEntity entity,
				IEnumerable<Category> categories,
				int priority)
			{
				var names = categories.Select(x => x.Name).ToHashSet();
				var entries = HelpEntries
					.GetHelpModules()
					.Where(x => names.Contains(x.Category));
				return Modify(entity, entries, priority);
			}
		}
	}

	/*
	[LocalizedGroup(nameof(Groups.ShowGuildSettings))]
	[LocalizedAlias(nameof(Aliases.ShowGuildSettings))]
	[LocalizedSummary(nameof(Summaries.ShowGuildSettings))]
	[Meta("b6ee91c4-05dc-4017-a08f-0c1478435179", IsEnabled = true)]
	[RequireGenericGuildPermissions]
	public sealed class ShowGuildSettings : ReadOnlySettingsModule<IGuildSettings>
	{
		protected override IGuildSettings Settings => Context.Settings;

		[LocalizedCommand(nameof(Groups.Json))]
		[LocalizedAlias(nameof(Aliases.Json))]
		[Priority(1)]
		public Task<RuntimeResult> Json()
			=> Responses.GuildSettings.DisplayJson(Settings);

		[LocalizedCommand(nameof(Groups.Names))]
		[LocalizedAlias(nameof(Aliases.Names))]
		[Priority(1)]
		public Task<RuntimeResult> Names()
			=> Responses.GuildSettings.DisplayNames(Settings);

		[LocalizedCommand(nameof(Groups.All))]
		[LocalizedAlias(nameof(Aliases.All))]
		[Priority(1)]
		public Task<RuntimeResult> All()
			=> Responses.GuildSettings.DisplaySettings(Context.Client, Context.Guild, Settings);

		[Command]
		public Task<RuntimeResult> Command([Remainder, GuildSettingName] string name)
			=> Responses.GuildSettings.DisplaySetting(Context.Client, Context.Guild, Settings, name);
	}

	[LocalizedGroup(nameof(Groups.ResetGuildSettings))]
	[LocalizedAlias(nameof(Aliases.ResetGuildSettings))]
	[LocalizedSummary(nameof(Summaries.ResetGuildSettings))]
	[Meta("316df0fc-1c5e-40fe-8580-7b8ca5f63b43", IsEnabled = true)]
	[RequireGuildPermissions]
	public sealed class ResetGuildSettings : SettingsModule<IGuildSettings>
	{
		protected override IGuildSettings Settings => Context.Settings;

		[LocalizedCommand(nameof(Groups.All))]
		[LocalizedAlias(nameof(Aliases.All))]
		[Priority(1)]
		public Task<RuntimeResult> All()
		{
			foreach (var setting in Settings.GetSettingNames())
			{
				Settings.ResetSetting(setting);
			}
			return Responses.GuildSettings.ResetAll();
		}

		[Command]
		public Task<RuntimeResult> Command([Remainder, GuildSettingName] string name)
		{
			Settings.ResetSetting(name);
			return Responses.GuildSettings.Reset(name);
		}
	}

	[LocalizedGroup(nameof(Groups.ModifyIgnoredCommandChannels))]
	[LocalizedAlias(nameof(Aliases.ModifyIgnoredCommandChannels))]
	[LocalizedSummary(nameof(Summaries.ModifyIgnoredCommandChannels))]
	[Meta("e485777b-1b3f-411a-afd7-59f24858cd24", IsEnabled = true, CanToggle = false)]
	[RequireGuildPermissions]
	public sealed class ModifyIgnoredCommandChannels : SettingsModule<IGuildSettings>
	{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public IHelpEntryService HelpEntries { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

		protected override IGuildSettings Settings => Context.Settings;
		//protected override string SettingName => nameof(IGuildSettings.IgnoredCommandChannels);

		[Command]
		public async Task Command(
			bool enable,
			[ValidateTextChannel(FromContext = true)] SocketTextChannel channel)
			=> await ModifyCollectionAsync(x => x.IgnoredCommandChannels, enable, channel.Id).ConfigureAwait(false);

		[ImplicitCommand, ImplicitAlias]
		public Task Category(bool enable, [ValidateCommandCategory] string category, [ValidateTextChannel(FromContext = true)] SocketTextChannel channel)
		{
			throw new NotImplementedException();
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
			if (!Settings.CommandSettings.ModifyOverride(helpEntry, channel, enable))
			{
				return ReplyErrorAsync($"`{helpEntry.Name}` is already {(enable ? "unignored" : "ignored")} on `{channel.Format()}`.");
			}
			return ReplyTimedAsync($"Successfully {(enable ? "unignored" : "ignored")} `{helpEntry.Name}` on `{channel.Format()}`.");
		}

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
					await MessageUtils.SendErrorMessageAsync(Context, error).ConfigureAwait(false);
					return;
				}

				var embed = new EmbedWrapper
				{
					Title = $"Permissions for {user.Format()}",
					Description = $"`{string.Join("`, `", EnumUtils.GetFlagNames((GuildPermission)botUser.Permissions))}`"
				};
				await MessageUtils.SendMessageAsync(Context.Channel, null, embed).ConfigureAwait(false);
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
					await MessageUtils.SendErrorMessageAsync(Context, error).ConfigureAwait(false);
					return;
				}

				var modifiedPerms = string.Join("`, `", botUser.ModifyPermissions(add, (IGuildUser)Context.User, permissions));
				var resp = $"Successfully {(add ? "removed" : "added")} the following bot permissions on `{user.Format()}`: `{modifiedPerms}`.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).ConfigureAwait(false);
			}

			protected override IGuildSettings GetSettings() => Context.GuildSettings;
		}
}*/
}