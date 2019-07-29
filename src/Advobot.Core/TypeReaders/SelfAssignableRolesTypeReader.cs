using Advobot.Attributes;
using Advobot.Classes.Modules;
using Advobot.Services.GuildSettings.Settings;
using AdvorangesUtils;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// Attempst to find a self assignable role group in the guild settings.
	/// </summary>
	[TypeReaderTargetType(typeof(SelfAssignableRoles))]
	public sealed class SelfAssignableRolesTypeReader : TypeReader<int, AdvobotCommandContext>
	{
		/// <inheritdoc />
		public override AsyncTryConverter<int> TryConverter
			=> (_1, input, _2) => Task.FromResult((int.TryParse(input, out var num), num));

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
