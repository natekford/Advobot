using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Classes;
using Advobot.Services.GuildSettings;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

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
		public override async Task<TypeReaderResult> ReadAsync(
			ICommandContext context,
			string input,
			IServiceProvider services)
		{
			var result = await _RoleTypeReader.ReadAsync(context, input, services).CAF();
			if (!result.IsSuccess)
			{
				return result;
			}
			var role = (IRole)result.BestMatch;

			var settingsFactory = services.GetRequiredService<IGuildSettingsFactory>();
			var settings = await settingsFactory.GetOrCreateAsync(context.Guild).CAF();
			var matches = settings.SelfAssignableGroups.SelectWhere(
				x => x.Roles.Contains(role.Id),
				x => new SelfAssignableRole(x, role)).ToArray();
			return TypeReaderUtils.SingleValidResult(matches, "self assignable roles", input);
		}
	}
}
