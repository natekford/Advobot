using Discord;
using Discord.Commands;

namespace Advobot.TypeReaders;

/// <summary>
/// Finds a channel based on position and type.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ChannelPositionTypeReader<T> : PositionTypeReader<T> where T : IGuildChannel
{
	/// <inheritdoc />
	public override string ObjectTypeName => "channels";

	/// <inheritdoc />
	protected override async Task<IReadOnlyList<T>> GetObjectsWithPositionAsync(
		ICommandContext context,
		int position)
	{
		var channels = await context.Guild.GetChannelsAsync().ConfigureAwait(false);
		return [.. channels.OfType<T>().Where(x => x.Position == position)];
	}
}