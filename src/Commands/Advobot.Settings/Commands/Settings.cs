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

namespace Advobot.Settings.Commands;

[LocalizedCategory(nameof(Names.SettingsCategory))]
public sealed class Settings : AdvobotModuleBase
{
	[LocalizedCommand(nameof(Names.ModifyCommands), nameof(Names.ModifyCommandsAlias))]
	[LocalizedSummary(nameof(Summaries.ModifyCommandsSummary))]
	[Meta("6fb02198-9eab-4e44-a59a-7ba7f7317c10", IsEnabled = true, CanToggle = false)]
	[RequireGuildPermissions]
	public sealed class ModifyCommands : AdvobotModuleBase
	{
		[LocalizedCommand(nameof(Names.Clear), nameof(Names.ClearAlias))]
		[LocalizedSummary(nameof(Summaries.ModifyCommandsClearSummary))]
		public sealed class Clear() : ModifyCommandsModuleBase(null)
		{
			[Command]
			public Task<AdvobotResult> All(CommandOverrideEntity entity)
				=> ModifyAllAsync(entity, 0);

			[Command]
			public Task<AdvobotResult> Select(
				CommandOverrideEntity entity,
				[OverrideTypeReader<CommandsNameTypeReader>]
				[Remainder]
				IReadOnlyCollection<IImmutableCommand> commands
			) => ModifyAsync(entity, commands, 0);
		}

		[LocalizedCommand(nameof(Names.Disable), nameof(Names.DisableAlias))]
		[LocalizedSummary(nameof(Summaries.ModifyCommandsDisableSummary))]
		public sealed class Disable() : ModifyCommandsModuleBase(false)
		{
			[Command]
			public Task<AdvobotResult> All(
				int priority,
				CommandOverrideEntity entity
			) => ModifyAllAsync(entity, priority);

			[Command]
			public Task<AdvobotResult> Select(
				int priority,
				CommandOverrideEntity entity,
				[OverrideTypeReader<CommandsNameTypeReader>]
				[Remainder]
				IReadOnlyCollection<IImmutableCommand> commands
			) => ModifyAsync(entity, commands, priority);
		}

		[LocalizedCommand(nameof(Names.Enable), nameof(Names.EnableAlias))]
		[LocalizedSummary(nameof(Summaries.ModifyCommandsEnableSummary))]
		public sealed class Enable() : ModifyCommandsModuleBase(true)
		{
			[Command]
			public Task<AdvobotResult> All(
				int priority,
				CommandOverrideEntity entity
			) => ModifyAllAsync(entity, priority);

			[Command]
			public Task<AdvobotResult> Select(
				int priority,
				CommandOverrideEntity entity,
				[OverrideTypeReader<CommandsNameTypeReader>]
				[Remainder]
				IReadOnlyCollection<IImmutableCommand> commands
			) => ModifyAsync(entity, commands, priority);
		}

		public abstract class ModifyCommandsModuleBase(bool? shouldEnable) : SettingsModuleBase
		{
			[InjectService]
			public required CommandService CommandService { get; set; }
			protected bool? ShouldEnable { get; } = shouldEnable;

			protected Task<AdvobotResult> ModifyAllAsync(CommandOverrideEntity entity, int priority)
				=> ModifyAsync(entity, CommandService.Commands, priority);

			protected async Task<AdvobotResult> ModifyAsync(
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
					return Responses.Settings.ModifiedCommands(commands, priority, ShouldEnable.Value);
				}
				else
				{
					await Db.DeleteCommandOverridesAsync(overrides).ConfigureAwait(false);
					return Responses.Settings.ClearedCommands(commands);
				}
			}
		}
	}
}