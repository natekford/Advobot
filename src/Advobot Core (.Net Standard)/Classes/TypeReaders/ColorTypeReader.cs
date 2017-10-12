using Discord;
using Discord.Commands;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// Attemps to create a <see cref="Color"/>.
	/// </summary>
	public sealed class ColorTypeReader : TypeReader
	{
		/// <summary>
		/// Input is tested as a color name, then hex, then RBG separated by back slashes.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
		{
			var color = GetColor(input);
			return color != null
				? Task.FromResult(TypeReaderResult.FromSuccess(color))
				: Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a matching color."));
		}

		public static Color? GetColor(string input)
		{
			Color? color = null;
			if (input == null)
			{
				return color;
			}
			//By name
			else if (Colors.COLORS.TryGetValue(input, out Color temp))
			{
				color = temp;
			}
			//By hex (trimming characters that are sometimes at the beginning of hex numbers)
			else if (uint.TryParse(input.TrimStart(new[] { '&', 'h', '#', '0', 'x' }), NumberStyles.HexNumber, null, out uint hex))
			{
				color = new Color(hex);
			}
			//By RGB
			else if (input.Contains('/'))
			{
				const byte MAX_VAL = 255;
				var colorRGB = input.Split('/');
				if (colorRGB.Length == 3 &&
					byte.TryParse(colorRGB[0], out byte r) &&
					byte.TryParse(colorRGB[1], out byte g) &&
					byte.TryParse(colorRGB[2], out byte b))
				{
					color = new Color(Math.Min(r, MAX_VAL), Math.Min(g, MAX_VAL), Math.Min(b, MAX_VAL));
				}
			}
			return color;
		}
	}
}
