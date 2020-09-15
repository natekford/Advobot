using System;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Services;
using Advobot.Settings.Database;
using Advobot.Settings.ReadOnlyModels;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.Settings.Service
{
	public class CommandValidator : ICommandValidator
	{
		private readonly ISettingsDatabase _Db;

		public CommandValidator(ISettingsDatabase db)
		{
			_Db = db;
		}

		public async Task<PreconditionResult> CanInvokeAsync(
			ICommandContext context,
			CommandInfo command)
		{
			// If we can't get an id, return success since the command isn't designed to work with this
			if (!(command.Attributes.SingleOrDefault(x => x is MetaAttribute) is MetaAttribute meta))
			{
				return PreconditionResult.FromSuccess();
			}
			var id = meta.Guid.ToString();

			var overrides = await _Db.GetCommandOverridesAsync(context.Guild.Id, id).CAF();
			foreach (var @override in overrides)
			{
				var entity = GetEntity(context, @override);
				if (entity == null)
				{
					continue;
				}
				if (@override.Enabled)
				{
					return PreconditionResult.FromSuccess();
				}
				return PreconditionResult.FromError($"Command is not enabled for {entity.Format()}.");
			}

			if (meta.IsEnabled)
			{
				return PreconditionResult.FromSuccess();
			}
			return PreconditionResult.FromError("Command is disabled by default.");
		}

		private static IEntity<ulong>? GetEntity(
			ICommandContext context,
			IReadOnlyCommandOverride @override)
		{
			static IEntity<ulong>? IsMatch(
				IEntity<ulong> entity,
				IReadOnlyCommandOverride @override)
				=> entity.Id == @override.TargetId ? entity : null;

			static IEntity<ulong>? IsRoleMatch(
				ICommandContext context,
				IReadOnlyCommandOverride @override)
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
}