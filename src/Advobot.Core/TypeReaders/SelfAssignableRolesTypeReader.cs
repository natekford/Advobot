using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// Attempst to find a self assignable role group in the guild settings.
	/// </summary>
	[TypeReaderTargetType(typeof(SelfAssignableRoles))]
	public sealed class SelfAssignableRolesTypeReader : TypeReader
	{
		/// <inheritdoc />
		public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			if (!int.TryParse(input, out var group))
			{
				return TypeReaderUtils.ParseFailedResult<int>();
			}

			var settingsFactory = services.GetRequiredService<IGuildSettingsFactory>();
			var settings = await settingsFactory.GetOrCreateAsync(context.Guild).CAF();
			var matches = settings.SelfAssignableGroups.Where(x => x.Group == group).ToArray();
			return TypeReaderUtils.SingleValidResult(matches, "self assignable role groups", input);
		}
	}
}
