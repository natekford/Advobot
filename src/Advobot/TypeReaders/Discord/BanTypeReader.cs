using Advobot.Modules;

using Discord;

using MorseCode.ITask;

using YACCS.Results;
using YACCS.TypeReaders;

namespace Advobot.TypeReaders.Discord;

/// <summary>
/// Attempts to find an <see cref="IBan"/>.
/// </summary>
[TypeReaderTargetTypes(typeof(IBan))]
public sealed class BanTypeReader : DiscordTypeReader<IBan>
{
	/// <inheritdoc />
	public override async ITask<ITypeReaderResult<IBan>> ReadAsync(
		IGuildContext context,
		ReadOnlyMemory<string> input)
	{
		var joined = Join(context, input);
		if (MentionUtils.TryParseUser(joined, out var id) || ulong.TryParse(joined, out id))
		{
			var ban = await context.Guild.GetBanAsync(id).ConfigureAwait(false);
			if (ban is not null)
			{
				return Success(ban);
			}
		}

		return TypeReaderResult<IBan>.ParseFailed.Result;
	}
}