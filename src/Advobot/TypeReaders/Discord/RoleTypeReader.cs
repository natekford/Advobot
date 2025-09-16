using Advobot.Modules;
using Advobot.Utilities;

using Discord;

using MorseCode.ITask;

using YACCS.Results;
using YACCS.TypeReaders;

namespace Advobot.TypeReaders.Discord;

/// <summary>
/// Attempts to find an <see cref="IRole"/>.
/// </summary>
[TypeReaderTargetTypes(typeof(IRole))]
public sealed class RoleTypeReader : DiscordTypeReader<IRole>
{
	/// <inheritdoc />
	public override async ITask<ITypeReaderResult<IRole>> ReadAsync(
		IGuildContext context,
		ReadOnlyMemory<string> input)
	{
		var joined = Join(context, input);
		if (MentionUtils.TryParseRole(joined, out var id) || ulong.TryParse(joined, out id))
		{
			var role = await context.Guild.GetRoleAsync(id).ConfigureAwait(false);
			if (role is not null)
			{
				return Success(role);
			}
		}

		var matches = context.Guild.Roles
			.Where(x => x.Name.CaseInsEquals(joined))
			.ToArray();
		return SingleValidResult(matches);
	}
}