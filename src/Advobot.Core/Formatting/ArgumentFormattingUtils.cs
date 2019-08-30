using System.Collections.Generic;
using System.Linq;

using AdvorangesUtils;

namespace Advobot.Formatting
{
	/// <summary>
	/// Utilities for formatting arguments.
	/// </summary>
	public static class ArgumentFormattingUtils
	{
		/// <summary>
		/// Put the object in markdown big code.
		/// </summary>
		public const string BIG_CODE = "```";

		/// <summary>
		/// Put the object in markdown bold.
		/// </summary>
		public const string BOLD = "**";

		/// <summary>
		/// Put the object in markdown code.
		/// </summary>
		public const string CODE = "`";

		/// <summary>
		/// What each piece of the format string will be joined with.
		/// </summary>
		public const string FORMAT_JOINER = "|";

		/// <summary>
		/// Put the object in markdown italics.
		/// </summary>
		public const string ITALICS = "_";

		/// <summary>
		/// Put the object in markdown strikethrough.
		/// </summary>
		public const string STRIKETHROUGH = "~~";

		/// <summary>
		/// Put the object in markdown underline.
		/// </summary>
		public const string UNDERLINE = "__";

		/// <summary>
		/// Formats the object as a title (in title case, with a colon at the end, and in bold).
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static RuntimeFormattedObject AsTitle(this string value)
		{
			var title = value.FormatTitle();
			if (!title.EndsWith(':'))
			{
				title += ":";
			}
			return RuntimeFormattedObject.Create(title, "**");
		}

		/// <summary>
		/// Joins the formats with <see cref="FORMAT_JOINER"/>.
		/// </summary>
		/// <param name="formats"></param>
		/// <returns></returns>
		public static string JoinFormats(IEnumerable<string> formats)
			=> formats.Join(FORMAT_JOINER);

		/// <summary>
		/// Creates an instance of <see cref="RuntimeFormattedObject"/> with no format.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static RuntimeFormattedObject NoFormatting(this object value)
			=> RuntimeFormattedObject.None(value);

		/// <summary>
		/// Converts a dictionary to a <see cref="DiscordFormattableStringCollection"/>.
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="source"></param>
		/// <param name="joiner"></param>
		/// <returns></returns>
		public static DiscordFormattableStringCollection ToDiscordFormattableStringCollection<TValue>(
			this IDictionary<string, TValue> source,
			string joiner = "\n")
		{
			var collection = new DiscordFormattableStringCollection();
			var values = source.ToArray();
			for (var i = 0; i < values.Length; ++i)
			{
				var kvp = values[i];
				if (i == source.Count - 1)
				{
					collection.Add($"{kvp.Key.AsTitle()} {kvp.Value}");
				}
				else
				{
					collection.Add($"{kvp.Key.AsTitle()} {kvp.Value}{joiner.NoFormatting()}");
				}
			}
			return collection;
		}

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
	}
}