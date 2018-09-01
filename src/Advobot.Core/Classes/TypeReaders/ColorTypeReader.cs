﻿using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// Attemps to create a <see cref="Color"/>.
	/// </summary>
	public sealed class ColorTypeReader : TypeReader
	{
		private static readonly ImmutableDictionary<string, Color> _Colors = typeof(Color)
			.GetFields(BindingFlags.Public | BindingFlags.Static)
			.ToDictionary(x => x.Name, x => (Color)x.GetValue(new Color()), StringComparer.OrdinalIgnoreCase)
			.ToImmutableDictionary();

		/// <summary>
		/// Input is tested as a color name, then hex, then RBG separated by back slashes.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			return ParseColor(input) is Color color
				? Task.FromResult(TypeReaderResult.FromSuccess(color))
				: Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a matching color."));
		}

		/// <summary>
		/// Attempts to parse a color from the input. If unable to parse, returns null.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static Color? ParseColor(string input)
		{
			if (input == null)
			{
				return null;
			}
			//By name
			if (_Colors.TryGetValue(input, out var temp))
			{
				return temp;
			}
			//By hex (trimming characters that are sometimes at the beginning of hex numbers)
			else if (uint.TryParse(input.Replace("0x", "").TrimStart('&', 'h', '#', 'x'), NumberStyles.HexNumber, null, out var hex))
			{
				return new Color(hex);
			}
			//By RGB
			else if (input.Contains('/'))
			{
				var colorRgb = input.Split('/');
				if (colorRgb.Length == 3 &&
					byte.TryParse(colorRgb[0], out var r) &&
					byte.TryParse(colorRgb[1], out var g) &&
					byte.TryParse(colorRgb[2], out var b))
				{
					return new Color(Math.Min(r, (byte)255), Math.Min(g, (byte)255), Math.Min(b, (byte)255));
				}
			}
			return null;
		}
	}
}
