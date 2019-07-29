using Advobot.Attributes;
using Advobot.Classes;
using Advobot.Modules;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// A type reader for self assignable roles.
	/// </summary>
	[TypeReaderTargetType(typeof(SelfAssignableRole))]
	public sealed class SelfAssignableRoleTypeReader : TypeReader<IAdvobotCommandContext>
	{
		private readonly RoleTypeReader<IRole> _RoleTypeReader = new RoleTypeReader<IRole>();

		/// <inheritdoc />
		public override async Task<TypeReaderResult> ReadAsync(IAdvobotCommandContext context, string input, IServiceProvider services)
		{
			var result = await _RoleTypeReader.ReadAsync(context, input, services).CAF();
			if (!result.IsSuccess)
			{
				return result;
			}

			var role = (IRole)result.BestMatch;
			if (!context.Settings.SelfAssignableGroups.TryGetSingle(x => x.Roles.Contains(role.Id), out var group))
			{
				return TypeReaderResult.FromError(CommandError.ObjectNotFound, $"`{role.Format()}` is not a self assignable role.");
			}

			var selfAssignable = new SelfAssignableRole(group, role);
			return TypeReaderResult.FromSuccess(selfAssignable);
		}
	}
}
