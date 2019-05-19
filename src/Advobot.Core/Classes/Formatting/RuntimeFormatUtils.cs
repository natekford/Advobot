using Advobot.Utilities;
using System.Collections.Generic;

namespace Advobot.Classes.Formatting
{
	/// <summary>
	/// Utilities for <see cref="RuntimeFormattedObject"/>.
	/// </summary>
	public static class RuntimeFormatUtils
	{
		/// <summary>
		/// What each piece of the format string will be joined with.
		/// </summary>
		public const string FORMAT_JOINER = "|";
		/// <summary>
		/// Put the object in title case.
		/// </summary>
		public const string TITLE = "title";
		/// <summary>
		/// Append a colon to object.
		/// </summary>
		public const string COLON = ":";
		/// <summary>
		/// Put the object in markdown code.
		/// </summary>
		public const string CODE = "`";
		/// <summary>
		/// Put the object in markdown big code.
		/// </summary>
		public const string BIG_CODE = "```";
		/// <summary>
		/// Put the object in markdown bold.
		/// </summary>
		public const string BOLD = "**";
		/// <summary>
		/// Put the object in markdown italics.
		/// </summary>
		public const string ITALICS = "_";
		/// <summary>
		/// Put the object in markdown underline.
		/// </summary>
		public const string UNDERLINE = "__";
		/// <summary>
		/// Put the object in markdown strikethrough.
		/// </summary>
		public const string STRIKETHROUGH = "~~";

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
		/// <summary>
		/// Creates an instance of <see cref="RuntimeFormattedObject"/> with the specified formats joined together with <see cref="FORMAT_JOINER"/>.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="formats"></param>
		/// <returns></returns>
		public static RuntimeFormattedObject WithFormats(this object value, params string[] formats)
			=> RuntimeFormattedObject.Create(value, JoinFormats(formats));
		/// <summary>
		/// Joins the formats with <see cref="FORMAT_JOINER"/>.
		/// </summary>
		/// <param name="formats"></param>
		/// <returns></returns>
		public static string JoinFormats(IEnumerable<string> formats)
			=> formats.Join(FORMAT_JOINER);
	}
}
