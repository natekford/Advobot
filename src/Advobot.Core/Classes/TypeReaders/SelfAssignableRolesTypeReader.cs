using Advobot.Classes.Attributes;
using Advobot.Classes.Modules;
using Advobot.Classes.Settings;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// Attempst to find a self assignable role group in the guild settings.
	/// </summary>
	[TypeReaderTargetType(typeof(SelfAssignableRoles))]
	public sealed class SelfAssignableRolesTypeReader : TypeReader<int, AdvobotCommandContext>
	{
		/// <inheritdoc />
		public override AsyncTryConverter<int, AdvobotCommandContext> TryConverter
			=> AsyncTryConverters.TryConvertIntAsync;

		/// <inheritdoc />
		public override Task<TypeReaderResult> ReadAsync(AdvobotCommandContext context, int input, IServiceProvider services)
		{
			if (context.GuildSettings.SelfAssignableGroups.TryGetSingle(x => x.Group == input, out var group))
			{
				return Task.FromResult(TypeReaderResult.FromSuccess(group));
			}
			return Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, $"There is no group with the group number `{input}`"));
		}
	}
}
