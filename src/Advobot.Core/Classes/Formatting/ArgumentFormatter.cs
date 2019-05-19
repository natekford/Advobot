using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Advobot.Classes.Formatting
{
	//TODO: make into service that accepts client meaning whenever it encounters a ulong it can parse it?
	/// <summary>
	/// Formats arguments with markdown.
	/// </summary>
	public class ArgumentFormatter : IFormatProvider, ICustomFormatter
	{
		/// <summary>
		/// What to join <see cref="IEnumerable{T}"/> with.
		/// </summary>
		public string Joiner { get; set; } = ", ";
		/// <summary>
		/// Specified formats.
		/// </summary>
		public IList<Formatter> Formats { get; set; } = new List<Formatter>();

		/// <inheritdoc />
		public object? GetFormat(Type formatType)
			=> formatType == typeof(ICustomFormatter) ? this : null;
		/// <inheritdoc />
		public string Format(string format, object arg, IFormatProvider formatProvider)
		{
			if (formatProvider != this)
			{
				throw new ArgumentException(nameof(formatProvider));
			}
			return Format(format, arg);
		}
		private string Format(string format, object arg)
		{
			if (arg is null)
			{
				return Format(format, "Nothing");
			}
			if (arg is RuntimeFormattedObject rtf)
			{
				if (format != null)
				{
					throw new InvalidOperationException($"{nameof(format)} should not be supplied if {nameof(RuntimeFormattedObject)} is being used.");
				}
				return Format(rtf.Format ?? "", rtf.Value);
			}
			if (arg is string str)
			{
				return Format(format, str);
			}
			if (arg is IEnumerable enumerable)
			{
				var sb = new StringBuilder();
				foreach (var item in enumerable)
				{
					if (sb.Length > 0)
					{
						sb.Append(Joiner);
					}
					sb.Append(Format(format, item));
				}
				return sb.Length > 0 ? sb.ToString() : Format(format, "None");
			}
			if (arg is ISnowflakeEntity snowflake)
			{
				return Format(format, snowflake.Format());
			}
			return Format(format, arg.ToString());
		}
		private string Format(string format, string arg)
		{
			var options = (format ?? "").Split(RuntimeFormatUtils.FORMAT_JOINER).Select(x => x.Trim()).ToArray();
			foreach (var kvp in Formats)
			{
				arg = kvp.Format(options, arg);
			}
			return arg;
		}

		/// <summary>
		/// Determines whether to add some formatting to a string.
		/// </summary>
		public class Formatter
		{
			/// <summary>
			/// Whether this is enabled.
			/// </summary>
			public bool Enabled { get; set; } = true;
			/// <summary>
			/// The format this formatter applies to.
			/// </summary>
			public string FormatString { get; }
			
			private readonly Func<string, string> _Modifier;

			/// <summary>
			/// Creates an instance of <see cref="Formatter"/>.
			/// </summary>
			/// <param name="format"></param>
			/// <param name="modifier"></param>
			public Formatter(string format, Func<string, string> modifier)
			{
				FormatString = format;
				_Modifier = modifier;
			}

			/// <summary>
			/// Returns a modified string if <paramref name="formats"/> contains the format this formatter is specified for or if this is enaabled and no formats are passed in.
			/// </summary>
			/// <param name="formats"></param>
			/// <param name="arg"></param>
			/// <returns></returns>
			public string Format(IReadOnlyCollection<string> formats, string arg)
				=> formats.Contains(FormatString) || (formats.Count == 0 && Enabled) ? _Modifier(arg) : arg;
		}
	}
}
