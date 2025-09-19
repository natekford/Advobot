using Advobot.Attributes;
using Advobot.Modules;
using Advobot.Services;
using Advobot.Settings.Database;
using Advobot.Settings.Database.Models;
using Advobot.Utilities;

using Discord;

using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Results;

namespace Advobot.Settings.Service;

public class CommandValidator(SettingsDatabase db) : ICommandValidator
{
	public async Task<IResult> CanInvokeAsync(
		IImmutableCommand command,
		IGuildContext context)
	{
		static IResult MetaResult(MetaAttribute meta)
		{
			if (meta.IsEnabled)
			{
				return Result.EmptySuccess;
			}
			return Result.Failure("This command is disabled by default.");
		}

		// If we can't get an id, return success since the command isn't designed to work with this
		if (command.GetAttributes<MetaAttribute>().SingleOrDefault() is not MetaAttribute meta)
		{
			return Result.EmptySuccess;
		}
		// Can't toggle, so will always be default
		// If changed from toggleable to not toggle, we also want to use the default value
		// instead of a potentially unchangable value in the database
		if (!meta.CanToggle)
		{
			return MetaResult(meta);
		}

		var id = command.PrimaryId;
		var overrides = await db.GetCommandOverridesAsync(context.Guild.Id, id).ConfigureAwait(false);
		foreach (var @override in overrides)
		{
			var entity = GetEntity(context, @override);
			if (entity is null)
			{
				continue;
			}
			if (@override.Enabled)
			{
				return Result.EmptySuccess;
			}
			return Result.Failure($"`{entity.Format()}` is not allowed to execute this command.");
		}
		return MetaResult(meta);
	}

	private static IEntity<ulong>? GetEntity(
		IGuildContext context,
		CommandOverride @override)
	{
		static IEntity<ulong>? IsMatch(
			IEntity<ulong> entity,
			CommandOverride @override)
			=> entity.Id == @override.TargetId ? entity : null;

		static IEntity<ulong>? IsRoleMatch(
			IGuildContext context,
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