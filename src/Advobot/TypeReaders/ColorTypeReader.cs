using Discord;

using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;

using YACCS.TypeReaders;

namespace Advobot.TypeReaders;

/// <summary>
/// Attemps to create a <see cref="Color"/>.
/// </summary>
[TypeReaderTargetTypes(typeof(Color))]
public sealed class ColorTypeReader() : TryParseTypeReader<Color>(TryParse)
{
	private static readonly ImmutableDictionary<string, Color> _Colors = typeof(Color)
		.GetFields(BindingFlags.Public | BindingFlags.Static)
		.Where(x => x.FieldType == typeof(Color))
		.ToDictionary(x => x.Name, x => (Color)x.GetValue(null)!, StringComparer.OrdinalIgnoreCase)
		.ToImmutableDictionary();
	private static readonly char[] _SplitChars = ['/', '-', ','];
	private static readonly char[] _TrimChars = ['&', 'h', '#', 'x'];

	private static bool TryParse(string s, out Color result)
	{
		if (s is null)
		{
			result = default;
			return true;
		}
		// By name
		if (_Colors.TryGetValue(s, out result))
		{
			return true;
		}
		// By hex (trimming characters that are sometimes at the beginning of hex numbers)
		var trimmed = s.Replace("0x", "").TrimStart(_TrimChars);
		if (uint.TryParse(trimmed, NumberStyles.HexNumber, null, out var hex))
		{
			result = new(hex);
			return true;
		}
		// By RGB
		foreach (var c in _SplitChars)
		{
			var split = s.Split(c);
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
}