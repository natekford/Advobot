using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// Attemps to create a <see cref="Color"/>.
	/// </summary>
	[TypeReaderTargetType(typeof(Color))]
	public sealed class ColorTypeReader : TypeReader
	{
		private static readonly ImmutableDictionary<string, Color> _Colors = typeof(Color)
			.GetFields(BindingFlags.Public | BindingFlags.Static)
			.ToDictionary(x => x.Name, x => (Color)x.GetValue(null), StringComparer.OrdinalIgnoreCase)
			.ToImmutableDictionary();

		/// <summary>
		/// Attempts to parse a color from the input. If unable to parse, returns null.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryParseColor(string input, out Color result)
		{
			if (input == null)
			{
				result = default;
				return true;
			}
			//By name
			if (_Colors.TryGetValue(input, out result))
			{
				return true;
			}
			//By hex (trimming characters that are sometimes at the beginning of hex numbers)
			var trimmed = input.Replace("0x", "").TrimStart('&', 'h', '#', 'x');
			if (uint.TryParse(trimmed, NumberStyles.HexNumber, null, out var hex))
			{
				result = new(hex);
				return true;
			}
			//By RGB
			foreach (var c in new[] { '/', '-', ',' })
			{
				var split = input.Split(c);
				if (split.Length != 3)
				{
					continue;
				}
				var rgb = split
					.Select(x => (Valid: byte.TryParse(x, out var val), Value: val))
					.Where(x => x.Valid)
					.ToArray();
				if (rgb.Length == 3)
				{
					result = new(rgb[0].Value, rgb[1].Value, rgb[2].Value);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Input is tested as a color name, then hex, then RBG separated by back slashes.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> ReadAsync(
			ICommandContext context,
			string input,
			IServiceProvider services)
		{
			if (TryParseColor(input, out var color))
			{
				return TypeReaderResult.FromSuccess(color).AsTask();
			}
			return TypeReaderUtils.ParseFailedResult<Color>().AsTask();
		}
	}
}