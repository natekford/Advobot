using Advobot.Attributes;
using Advobot.Modules;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;
using Advobot.Settings.Database.Models;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Building;
using YACCS.Commands.Models;
using YACCS.Localization;
using YACCS.TypeReaders;

using static Advobot.Settings.Responses.Settings;

namespace Advobot.Settings.Commands;

[LocalizedCategory(nameof(Settings))]
public sealed class Settings : AdvobotModuleBase
{
	[LocalizedCommand(nameof(Groups.ModifyCommands), nameof(Aliases.ModifyCommands))]
	[LocalizedSummary(nameof(Summaries.ModifyCommands))]
	[Id("6fb02198-9eab-4e44-a59a-7ba7f7317c10")]
	[Meta(IsEnabled = true, CanToggle = false)]
	[RequireGuildPermissions]
	public sealed class ModifyCommands : AdvobotModuleBase
	{
		[LocalizedCommand(nameof(Groups.Clear), nameof(Aliases.Clear))]
		[LocalizedSummary(nameof(Summaries.ModifyCommandsClear))]
		public sealed class Clear : ModifyCommandsModuleBase
		{
			protected override bool? ShouldEnable => null;

			[Command]
			public Task<AdvobotResult> Command(CommandOverrideEntity entity)
				=> ModifyAll(entity, 0);

			[Command]
			public Task<AdvobotResult> Command(
				CommandOverrideEntity entity,
				[OverrideTypeReader<CommandsNameTypeReader>]
				[Remainder]
				IReadOnlyCollection<IImmutableCommand> commands
			) => Modify(entity, commands, 0);
		}

		[LocalizedCommand(nameof(Groups.Disable), nameof(Aliases.Disable))]
		[LocalizedSummary(nameof(Summaries.ModifyCommandsDisable))]
		public sealed class Disable : ModifyCommandsModuleBase
		{
			protected override bool? ShouldEnable => false;

			[Command]
			public Task<AdvobotResult> Command(
				int priority,
				CommandOverrideEntity entity
			) => ModifyAll(entity, priority);

			[Command]
			public Task<AdvobotResult> Command(
				int priority,
				CommandOverrideEntity entity,
				[OverrideTypeReader<CommandsNameTypeReader>]
				[Remainder]
				IReadOnlyCollection<IImmutableCommand> commands
			) => Modify(entity, commands, priority);
		}

		[LocalizedCommand(nameof(Groups.Enable), nameof(Aliases.Enable))]
		[LocalizedSummary(nameof(Summaries.ModifyCommandsEnable))]
		public sealed class Enable : ModifyCommandsModuleBase
		{
			protected override bool? ShouldEnable => true;

			[Command]
			public Task<AdvobotResult> Command(
				int priority,
				CommandOverrideEntity entity
			) => ModifyAll(entity, priority);

			[Command]
			public Task<AdvobotResult> Command(
				int priority,
				CommandOverrideEntity entity,
				[OverrideTypeReader<CommandsNameTypeReader>]
				[Remainder]
				IReadOnlyCollection<IImmutableCommand> commands
			) => Modify(entity, commands, priority);
		}

		public abstract class ModifyCommandsModuleBase : SettingsModuleBase
		{
			[InjectService]
			public required CommandService CommandService { get; set; }
			protected abstract bool? ShouldEnable { get; }

			protected async Task<AdvobotResult> Modify(
				CommandOverrideEntity entity,
				IEnumerable<IImmutableCommand> commands,
				int priority)
			{
				var overrides = commands.Select(x => new CommandOverride(entity)
				{
					CommandId = x.PrimaryId,
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

			protected Task<AdvobotResult> ModifyAll(CommandOverrideEntity entity, int priority)
				=> Modify(entity, CommandService.Commands, priority);
		}
	}

#warning reenable
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