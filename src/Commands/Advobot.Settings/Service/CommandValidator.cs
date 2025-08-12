using Advobot.Attributes;
using Advobot.Services;
using Advobot.Settings.Database;
using Advobot.Settings.Database.Models;
using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.Settings.Service;

public class CommandValidator(ISettingsDatabase db) : ICommandValidator
{
	private readonly ISettingsDatabase _Db = db;

	public async Task<PreconditionResult> CanInvokeAsync(
		ICommandContext context,
		CommandInfo command)
	{
		static PreconditionResult MetaResult(MetaAttribute meta)
		{
			if (meta.IsEnabled)
			{
				return PreconditionResult.FromSuccess();
			}
			return PreconditionResult.FromError("Command is disabled by default.");
		}

		// If we can't get an id, return success since the command isn't designed to work with this
		var attributes = command.Module.Attributes;
		if (attributes.SingleOrDefault(x => x is MetaAttribute) is not MetaAttribute meta)
		{
			return PreconditionResult.FromSuccess();
		}
		// Can't toggle, so will always be default
		// If changed from toggleable to not toggle, we also want to use the default value
		// instead of a potentially unchangable value in the database
		if (!meta.CanToggle)
		{
			return MetaResult(meta);
		}

		var id = meta.Guid.ToString();
		var overrides = await _Db.GetCommandOverridesAsync(context.Guild.Id, id).ConfigureAwait(false);
		foreach (var @override in overrides)
		{
			var entity = GetEntity(context, @override);
			if (entity is null)
			{
				continue;
			}
			if (@override.Enabled)
			{
				return PreconditionResult.FromSuccess();
			}
			return PreconditionResult.FromError($"Command is not enabled for `{entity.Format()}`.");
		}
		return MetaResult(meta);
	}

	private static IEntity<ulong>? GetEntity(
		ICommandContext context,
		CommandOverride @override)
	{
		static IEntity<ulong>? IsMatch(
			IEntity<ulong> entity,
			CommandOverride @override)
			=> entity.Id == @override.TargetId ? entity : null;

		static IEntity<ulong>? IsRoleMatch(
			ICommandContext context,
			CommandOverride @override)
		{
			if (context.User is IGuildUser user && user.RoleIds.Contains(@override.TargetId))
			{
				return context.Guild.GetRole(@override.TargetId);
			}
			return null;
		}

		return @override.TargetType switch
		{
			CommandOverrideType.User => IsMatch(context.User, @override),
			CommandOverrideType.Role => IsRoleMatch(context, @override),
			CommandOverrideType.Channel => IsMatch(context.Channel, @override),
			CommandOverrideType.Guild => IsMatch(context.Guild, @override),
			_ => null,
		};
	}
}