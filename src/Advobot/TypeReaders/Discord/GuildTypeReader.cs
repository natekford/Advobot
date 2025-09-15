using Advobot.Modules;
using Advobot.Utilities;

using Discord;

using MorseCode.ITask;

using YACCS.TypeReaders;

namespace Advobot.TypeReaders.Discord;

/// <summary>
/// Attempts to find an <see cref="IGuild"/>.
/// </summary>
[TypeReaderTargetTypes(typeof(IGuild))]
public sealed class GuildTypeReader : DiscordTypeReader<IGuild>
{
	/// <inheritdoc />
	public override async ITask<ITypeReaderResult<IGuild>> ReadAsync(
		IGuildContext context,
		ReadOnlyMemory<string> input)
	{
		var joined = Join(context, input);
		if (ulong.TryParse(joined, out var id))
		{
			var guild = await context.Client.GetGuildAsync(id).ConfigureAwait(false);
			if (guild != null)
			{
				return Success(guild);
			}
		}

		var matches = (await context.Client.GetGuildsAsync().ConfigureAwait(false))
			.Where(x => x.Name.CaseInsEquals(joined))
			.ToArray();
		return SingleValidResult(matches);
	}
}