﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Advobot.Utilities;

using AdvorangesUtils;

using Discord;

namespace Advobot.Formatting
{
	/// <summary>
	/// Formats arguments with markdown.
	/// </summary>
	public class ArgumentFormatter : IFormatProvider, ICustomFormatter
	{
		private readonly ICollection<IObjectToStringConverter> _ObjectConverters;

		/// <summary>
		/// Specified formats.
		/// </summary>
		public IList<FormatApplier> Formats { get; set; } = new List<FormatApplier>();

		/// <summary>
		/// What to join <see cref="IEnumerable{T}"/> with.
		/// </summary>
		public string Joiner { get; set; } = ", ";

		/// <summary>
		/// Creates an instance of <see cref="ArgumentFormatter"/>.
		/// </summary>
		public ArgumentFormatter()
		{
			_ObjectConverters = new List<IObjectToStringConverter>
			{
				new NullToStringConverter(.1, f => FormatString(f, "Nothing")),
				new ObjectToStringConverter<object>(.1, (f, v) => FormatString(f, v.ToString())),
				new ObjectToStringConverter<string>(1, FormatString),
				new ObjectToStringConverter<Enum>(1, (f, v) => FormatEnumerable(f, EnumUtils.GetFlagNames(v))),
				new ObjectToStringConverter<RuntimeFormattedObject>(1, (f, v) => Format(v.Format ?? f, v.Value)),
				new ObjectToStringConverter<IGuildFormattable>(1, (f, v) => Format(f, v.GetFormattableString())),
				new ObjectToStringConverter<IDiscordFormattableString>(1, (_, v) => v.ToString("", this)),
				new ObjectToStringConverter<ISnowflakeEntity>(1, (f, v) => FormatString(f, v.Format())),
				new ObjectToStringConverter<IEnumerable>(.5, FormatEnumerable),
			};
		}

		/// <inheritdoc />
		public string Format(string format, object arg, IFormatProvider formatProvider)
		{
			if (formatProvider != this)
			{
				throw new ArgumentException(nameof(formatProvider));
			}
			return Format(format, arg);
		}

		/// <inheritdoc />
		public object? GetFormat(Type formatType)
			=> formatType == typeof(ICustomFormatter) ? this : null;

		private string Format(string format, object arg)
		{
			var highestPriority = _ObjectConverters
				.Where(x => x.CanFormat(arg))
				.GroupBy(x => x.Priority)
				.OrderBy(x => x.Key)
				.Last()
				.ToArray();
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
			var validFormatOptions = Formats.Select(x => x.FormatName);
			var options = (format ?? "")
				.Split(ArgumentFormattingUtils.FORMAT_JOINER)
				.Select(x => x.Trim())
				.Where(x => x == "G" || validFormatOptions.Contains(x))
				.ToArray();
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
			private readonly Func<string, string> _Func;

			public double Priority { get; }

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
			private readonly Func<string, T, string> _Func;

			public double Priority { get; }

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