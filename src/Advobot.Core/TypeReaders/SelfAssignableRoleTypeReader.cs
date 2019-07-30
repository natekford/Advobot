using Advobot.Attributes;
using Advobot.Classes;
using Advobot.Services.GuildSettings;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// A type reader for self assignable roles.
	/// </summary>
	[TypeReaderTargetType(typeof(SelfAssignableRole))]
	public sealed class SelfAssignableRoleTypeReader : TypeReader
	{
		private static readonly TypeReader _RoleTypeReader = new RoleTypeReader<IRole>();

		/// <inheritdoc />
		public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			var result = await _RoleTypeReader.ReadAsync(context, input, services).CAF();
			if (!result.IsSuccess)
			{
				return result;
			}
			var role = (IRole)result.BestMatch;

			var settingsFactory = services.GetRequiredService<IGuildSettingsFactory>();
			var settings = await settingsFactory.GetOrCreateAsync(context.Guild).CAF();
			if (!settings.SelfAssignableGroups.TryGetSingle(x => x.Roles.Contains(role.Id), out var group))
			{
				return TypeReaderResult.FromError(CommandError.ObjectNotFound, $"`{role.Format()}` is not a self assignable role.");
			}
			return TypeReaderResult.FromSuccess(new SelfAssignableRole(group, role));
		}
	}
}
