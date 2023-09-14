using Discord;

namespace Advobot.Interactivity.TryParsers;

/// <summary>
/// Attempts to parse an index from a message.
/// </summary>
/// <remarks>
/// Creates an instance of <see cref="IndexTryParser"/>.
/// </remarks>
/// <param name="minVal"></param>
/// <param name="maxVal"></param>
public sealed class IndexTryParser(int minVal, int maxVal) : IMessageTryParser<int>
{
	private readonly int _MaxVal = maxVal;
	private readonly int _MinVal = minVal;

	/// <inheritdoc />
	public ValueTask<Optional<int>> TryParseAsync(IMessage message)
	{
		if (!int.TryParse(message.Content, out var position))
		{
			return new(Optional<int>.Unspecified);
		}

		var index = position - 1;
		if (index >= _MinVal && index <= _MaxVal)
		{
			return new(index);
		}
		return new(Optional<int>.Unspecified);
	}
}