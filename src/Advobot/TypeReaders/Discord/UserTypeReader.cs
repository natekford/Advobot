using Advobot.Modules;
using Advobot.Utilities;

using Discord;

using MorseCode.ITask;

using YACCS.TypeReaders;

namespace Advobot.TypeReaders.Discord;

/// <summary>
/// Attemps to find an <see cref="IGuildUser"/>.
/// </summary>
[TypeReaderTargetTypes(typeof(IGuildUser))]
public sealed class UserTypeReader : DiscordTypeReader<IGuildUser>
{
	/// <inheritdoc />
	public override async ITask<ITypeReaderResult<IGuildUser>> ReadAsync(
		IGuildContext context,
		ReadOnlyMemory<string> input)
	{
		var joined = Join(context, input);
		if (MentionUtils.TryParseUser(joined, out var id) || ulong.TryParse(joined, out id))
		{
			var user = await context.Guild.GetUserAsync(id, CacheMode.CacheOnly).ConfigureAwait(false);
			if (user is not null)
			{
				return Success(user);
			}
		}

		var matches = (await context.Guild.GetUsersAsync(CacheMode.CacheOnly).ConfigureAwait(false))
			.Where(x =>
			{
				return x.GlobalName.CaseInsEquals(joined)
					|| x.Username.CaseInsEquals(joined)
					|| x.Nickname.CaseInsEquals(joined);
			})
			.ToArray();
		return SingleValidResult(matches);
	}
}