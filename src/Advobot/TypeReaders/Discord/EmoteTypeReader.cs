using Advobot.Modules;
using Advobot.Utilities;

using Discord;

using MorseCode.ITask;

using YACCS.TypeReaders;

namespace Advobot.TypeReaders.Discord;

/// <summary>
/// Attempts to find an <see cref="Emote"/>.
/// </summary>
[TypeReaderTargetTypes(typeof(Emote))]
public sealed class EmoteTypeReader : DiscordTypeReader<Emote>
{
	/// <inheritdoc />
	public override ITask<ITypeReaderResult<Emote>> ReadAsync(
		IGuildContext context,
		ReadOnlyMemory<string> input)
	{
		var joined = Join(context, input);
		if (Emote.TryParse(joined, out var emote))
		{
			var guildEmote = context.Guild.Emotes.FirstOrDefault(x => x.Id == emote.Id);
			return Success(guildEmote ?? emote).AsITask();
		}
		if (ulong.TryParse(joined, out var id))
		{
			var guildEmote = context.Guild.Emotes.FirstOrDefault(x => x.Id == id);
			if (guildEmote != null)
			{
				return Success(guildEmote).AsITask();
			}
		}

		var matches = context.Guild.Emotes
			.Where(x => x.Name.CaseInsEquals(joined))
			.ToArray();
		return SingleValidResult(matches).AsITask();
	}
}