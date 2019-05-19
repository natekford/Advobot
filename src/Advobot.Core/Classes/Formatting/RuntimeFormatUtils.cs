namespace Advobot.Classes.Formatting
{
	/// <summary>
	/// Utilities for <see cref="RuntimeFormattedObject"/>.
	/// </summary>
	public static class RuntimeFormatUtils
	{
		/// <summary>
		/// Creates an instance of <see cref="RuntimeFormattedObject"/> with no format.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static RuntimeFormattedObject NoFormatting(this object value)
			=> RuntimeFormattedObject.None(value);
		/// <summary>
		/// Creates an instance of <see cref="RuntimeFormattedObject"/> with the specified format.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="format"></param>
		/// <returns></returns>
		public static RuntimeFormattedObject WithFormat(this object value, string format)
			=> RuntimeFormattedObject.Create(value, format);
	}
}
