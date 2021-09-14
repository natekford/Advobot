
using Discord;

namespace Advobot.Interactivity.TryParsers
{
	/// <summary>
	/// Parses objects from messages.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IMessageTryParser<T>
	{
		/// <summary>
		/// Attempts to parse a value from the message.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public ValueTask<Optional<T>> TryParseAsync(IMessage message);
	}
}