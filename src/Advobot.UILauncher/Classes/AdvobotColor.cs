using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Media;

namespace Advobot.UILauncher.Classes
{
	internal class AdvobotColor
	{
		private static BrushConverter _Converter = new BrushConverter();
		private static ImmutableDictionary<string, Color> _Colors = typeof(System.Drawing.Color)
			.GetProperties(BindingFlags.Public | BindingFlags.Static)
			.Where(p => p.PropertyType == typeof(System.Drawing.Color))
			.ToDictionary(p => p.Name, p =>
			{
				var c = (System.Drawing.Color)p.GetValue(null, null);
				return CreateColorFromARGB(c.A, c.R, c.G, c.B);
			}).ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);

		public int A { get; }
		public int R { get; }
		public int G { get; }
		public int B { get; }
		public Color Color { get; }
		public bool IsValid { get; } = true;

		private AdvobotColor(string input)
		{
			var split = input.Split('/');
			if (split.Length == 3 && TryCreateColorFromStringRGB(split, out Color rgb))
			{
				Color = rgb;
			}
			else if (TryCreateColorFromStringName(input, out Color name))
			{
				Color = name;
			}
			else if (TryCreateColorFromStringHex(input, out Color hex))
			{
				Color = hex;
			}
			else
			{
				IsValid = false;
			}
			A = Color.A;
			R = Color.R;
			G = Color.G;
			B = Color.B;
		}
		public AdvobotColor(int value)
		{
			Color = CreateColorFromInt(value);
			A = Color.A;
			R = Color.R;
			G = Color.G;
			B = Color.B;
		}
		public AdvobotColor(byte r, byte g, byte b) : this(255, r, g, b) { }
		public AdvobotColor(byte a, byte r, byte g, byte b)
		{
			Color = CreateColorFromARGB(a, r, g, b);
			A = Color.A;
			R = Color.R;
			G = Color.G;
			B = Color.B;
		}

		private static bool TryCreateColorFromStringRGB(string[] rgb, out Color color)
		{
			if (rgb.Length == 3 && byte.TryParse(rgb[0], out var r) && byte.TryParse(rgb[1], out var g) && byte.TryParse(rgb[2], out var b))
			{
				color = CreateColorFromARGB(255, r, g, b);
				return true;
			}
			color = default;
			return false;
		}
		private static bool TryCreateColorFromStringName(string name, out Color color)
		{
			return _Colors.TryGetValue(name, out color);
		}
		private static bool TryCreateColorFromStringHex(string hex, out Color color)
		{
			if (uint.TryParse(hex.TrimStart(new[] { '&', 'h', '#', '0', 'x' }), NumberStyles.HexNumber, null, out uint h))
			{
				color = CreateColorFromInt((int)h);
				return true;
			}
			color = default;
			return false;
		}
		private static Color CreateColorFromInt(int value)
		{
			var color = System.Drawing.Color.FromArgb(value);
			return CreateColorFromARGB(color.A, color.R, color.G, color.B);
		}
		private static Color CreateColorFromARGB(byte a, byte r, byte g, byte b)
		{
			a = Math.Min(a, (byte)255);
			r = Math.Min(r, (byte)255);
			g = Math.Min(g, (byte)255);
			b = Math.Min(b, (byte)255);
			return Color.FromArgb(a, r, g, b);
		}

		public static bool TryCreateColor(string input, out AdvobotColor color)
		{
			color = new AdvobotColor(input);
			return color.IsValid;
		}
		public SolidColorBrush CreateBrush()
		{
			return new SolidColorBrush(Color);
		}
		public static SolidColorBrush CreateBrush(string input)
		{
			return _Converter.ConvertFrom(input) as SolidColorBrush;
		}
		public static bool CheckIfSameBrush(SolidColorBrush b1, SolidColorBrush b2)
		{
			return b1?.Color == b2?.Color && b1?.Opacity == b2?.Opacity;
		}
	}
}
