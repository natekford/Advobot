using Advobot.Utilities;
using Discord;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvorangesUtils;

namespace Advobot.Classes.Formatting
{
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
		public IList<FormatApplier> Formats { get; set; } = new List<FormatApplier>();

		private readonly ICollection<IObjectToStringConverter> _ObjectConverters;

		/// <summary>
		/// Creates an instance of <see cref="ArgumentFormatter"/>.
		/// </summary>
		public ArgumentFormatter()
		{
			_ObjectConverters = new List<IObjectToStringConverter>
			{
				new NullToStringConverter(.1, f => FormatString(f, "Nothing")),
				new ObjectToStringConverter<object>(.1, (f, v) => FormatString(f, v.ToString())),
				new ObjectToStringConverter<string>(1, (f, v) => FormatString(f, v)),
				new ObjectToStringConverter<Enum>(1, (f, v) => FormatEnumerable(f, EnumUtils.GetFlagNames(v))),
				new ObjectToStringConverter<RuntimeFormattedObject>(1, (f, v) => Format(v.Format ?? f, v.Value)),
				new ObjectToStringConverter<IGuildFormattable>(1, (f, v) => Format(f, v.GetFormattableString())),
				new ObjectToStringConverter<IDiscordFormattableString>(1, (f, v) => v.ToString("", this)),
				new ObjectToStringConverter<ISnowflakeEntity>(1, (f, v) => FormatString(f, v.Format())),
				new ObjectToStringConverter<IEnumerable>(.5, (f, v) => FormatEnumerable(f, v)),
			};
		}

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
			var applicableConverters = _ObjectConverters.Where(x => x.CanFormat(arg));
			var highestPriority = applicableConverters.GroupBy(x => x.Priority).OrderBy(x => x.Key).Last().ToArray();
			if (highestPriority.Length > 1)
			{
				throw new InvalidOperationException("Cannot decide between multiple object formatters with the same priority.");
			}
			return highestPriority[0].Format(format, arg);
		}
		private string FormatEnumerable(string format, IEnumerable arg)
		{
			var sb = new StringBuilder();
			foreach (var item in arg)
			{
				if (sb.Length > 0)
				{
					sb.Append(Joiner);
				}
				sb.Append(Format(format, item));
			}
			return sb.Length > 0 ? sb.ToString() : FormatString(format, "None");
		}
		private string FormatString(string format, string arg)
		{
			var options = (format ?? "").Split(ArgumentFormattingUtils.FORMAT_JOINER).Select(x => x.Trim()).ToArray();
			foreach (var kvp in Formats)
			{
				arg = kvp.ModifyString(options, arg);
			}
			return arg;
		}

		private interface IObjectToStringConverter
		{
			double Priority { get; }

			bool CanFormat(object obj);
			string Format(string format, object obj);
		}

		private readonly struct NullToStringConverter : IObjectToStringConverter
		{
			public double Priority { get; }

			private readonly Func<string, string> _Func;

			/// <summary>
			/// Creates an instance of <see cref="ObjectToStringConverter{T}"/>.
			/// </summary>
			/// <param name="priority"></param>
			/// <param name="func"></param>
			public NullToStringConverter(double priority, Func<string, string> func)
			{
				Priority = priority;
				_Func = func;
			}

			public bool CanFormat(object obj)
				=> obj is null;
			public string Format(string format)
				=> _Func(format);

			string IObjectToStringConverter.Format(string format, object obj)
				=> Format(format);
		}

		private readonly struct ObjectToStringConverter<T> : IObjectToStringConverter
		{
			public double Priority { get; }

			private readonly Func<string, T, string> _Func;

			/// <summary>
			/// Creates an instance of <see cref="ObjectToStringConverter{T}"/>.
			/// </summary>
			/// <param name="priority"></param>
			/// <param name="func"></param>
			public ObjectToStringConverter(double priority, Func<string, T, string> func)
			{
				Priority = priority;
				_Func = func;
			}

			public bool CanFormat(object obj)
				=> obj is T;
			public string Format(string format, T obj)
				=> _Func(format, obj);

			string IObjectToStringConverter.Format(string format, object obj)
			{
				if (!CanFormat(obj))
				{
					throw new ArgumentException(nameof(obj));
				}
				return Format(format, (T)obj);
			}
		}
	}
}
