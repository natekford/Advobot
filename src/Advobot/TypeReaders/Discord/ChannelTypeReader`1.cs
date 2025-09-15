using Advobot.Modules;
using Advobot.Utilities;

using Discord;

using MorseCode.ITask;

using YACCS.TypeReaders;

namespace Advobot.TypeReaders.Discord;

/// <summary>
/// Attempts to find an <see cref="ICategoryChannel"/>.
/// </summary>
[TypeReaderTargetTypes(typeof(ICategoryChannel))]
public sealed class CategoryChannelTypeReader : ChannelTypeReader<ICategoryChannel>;

/// <summary>
/// Attempts to find an <see cref="IGuildChannel"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class ChannelTypeReader<T> : DiscordTypeReader<T>
	where T : IGuildChannel
{
	/// <inheritdoc />
	public override async ITask<ITypeReaderResult<T>> ReadAsync(
		IGuildContext context,
		ReadOnlyMemory<string> input)
	{
		var joined = Join(context, input);
		if (MentionUtils.TryParseChannel(joined, out var id) || ulong.TryParse(joined, out id))
		{
			var channel = await context.Guild.GetChannelAsync(id, CacheMode.CacheOnly).ConfigureAwait(false);
			if (channel is T tChannel)
			{
				return Success(tChannel);
			}
		}

		var matches = (await context.Guild.GetChannelsAsync(CacheMode.CacheOnly).ConfigureAwait(false))
			.OfType<T>()
			.Where(x => x.Name.CaseInsEquals(joined))
			.ToArray();
		return SingleValidResult(matches);
	}
}

/// <summary>
/// Attempts to find an <see cref="IGuildChannel"/>.
/// </summary>
[TypeReaderTargetTypes(typeof(IGuildChannel))]
public sealed class GuildChannelTypeReader : ChannelTypeReader<IGuildChannel>;

/// <summary>
/// Attempts to find an <see cref="ITextChannel"/>.
/// </summary>
[TypeReaderTargetTypes(typeof(ITextChannel))]
public sealed class TextChannelTypeReader : ChannelTypeReader<ITextChannel>;

/// <summary>
/// Attempts to find an <see cref="IVoiceChannel"/>.
/// </summary>
[TypeReaderTargetTypes(typeof(IVoiceChannel))]
public sealed class VoiceChannelTypeReader : ChannelTypeReader<IVoiceChannel>;