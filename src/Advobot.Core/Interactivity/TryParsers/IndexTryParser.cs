using Discord;

namespace Advobot.Interactivity.TryParsers;

/// <summary>
/// Attempts to parse an index from a message.
/// </summary>
public sealed class IndexTryParser : IMessageTryParser<int>
{
	private readonly int _MaxVal;
	private readonly int _MinVal;

	/// <summary>
	/// Creates an instance of <see cref="IndexTryParser"/>.
	/// </summary>
	/// <param name="minVal"></param>
	/// <param name="maxVal"></param>
	public IndexTryParser(int minVal, int maxVal)
	{
		_MinVal = minVal;
		_MaxVal = maxVal;
	}

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