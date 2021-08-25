using System.Globalization;

namespace Advobot.UI.AbstractUI.Colors
{
	/// <summary>
	/// Specifies how to create a brush.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class BrushFactory<T>
	{
		/// <summary>
		/// Returns the default value of this brush type.
		/// </summary>
		public T Default => CreateBrush("#FF000000");

		/// <summary>
		/// Creates a brush from the input.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public T CreateBrush(string input)
			=> CreateBrush(ParseColorBytes(input));

		/// <summary>
		/// Creates a brush from ARGB.
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public abstract T CreateBrush(byte[] bytes);

		/// <summary>
		/// Returns the hex string representation of the brush.
		/// </summary>
		/// <param name="brush"></param>
		/// <returns></returns>
		public string FormatBrush(T brush)
		{
			var bytes = GetBrushBytes(brush);
			return $"#{bytes[0]:X2}{bytes[1]:X2}{bytes[2]:X2}{bytes[3]:X2}";
		}

		/// <summary>
		/// Gets the brush's ARGB bytes.
		/// </summary>
		/// <param name="brush"></param>
		/// <returns></returns>
		public abstract byte[] GetBrushBytes(T brush);

		/// <summary>
		/// Attempts to create a brush from the input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="brush"></param>
		/// <returns></returns>
		public bool TryCreateBrush(string input, out T brush)
		{
			var success = TryParseColorBytes(input, out var bytes);
			brush = success ? CreateBrush(bytes) : Default;
			return success;
		}

		/// <summary>
		/// Gets ARGB color bytes.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		private static byte[] ParseColorBytes(string input)
		{
			if (TryGetColorBytesARGB(input, out var rgb))
			{
				return rgb;
			}
			if (TryGetColorBytesHex(input, out var hex))
			{
				return hex;
			}
			throw new InvalidOperationException($"Unable to create a brush out of {input}.");
		}

		private static bool TryGetColorBytesARGB(string input, out byte[] bytes)
		{
			var split = input.Split('/');
			//1 or 2 or 5+ means invalid amount of bytes
			if (split.Length < 3 || split.Length > 4)
			{
				bytes = new byte[] { 0, 0, 0, 0 };
				return false;
			}

			//4 length even if only 3 parts, just set first part to 255 in that case to have a fully opaque color
			bytes = new byte[4];
			var noA = split.Length == 3;
			if (noA)
			{
				bytes[0] = byte.MaxValue;
			}

			for (var i = 0; i < split.Length; ++i)
			{
				if (!byte.TryParse(split[i], out var val))
				{
					bytes = new byte[] { 0, 0, 0, 0 };
					return false;
				}
				bytes[i + (noA ? 1 : 0)] = val;
			}

			return true;
		}

		private static bool TryGetColorBytesHex(string hex, out byte[] bytes)
		{
			var trimmed = hex.Replace("0x", "").TrimStart('&', 'h', '#', 'x');
			//If not 6 wide add in more 0s so the call right below doesn't mess with the colors except the alpha channel
			while (trimmed.Length < 6)
			{
				trimmed = "0" + trimmed;
			}
			//If not 8 wide then add in more F's to make the alpha channel opaque
			while (trimmed.Length < 8)
			{
				trimmed = "F" + trimmed;
			}

			if (uint.TryParse(trimmed, NumberStyles.HexNumber, null, out var h))
			{
				bytes = BitConverter.GetBytes(h);
				//Make sure the color won't come out backwards
				if (BitConverter.IsLittleEndian)
				{
					Array.Reverse(bytes);
				}
				return true;
			}
			bytes = new byte[] { 0, 0, 0, 0 };
			return false;
		}

		/// <summary>
		/// Attempts to get ARGB color bytes.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="bytes"></param>
		/// <returns></returns>
		private static bool TryParseColorBytes(string input, out byte[] bytes)
		{
			if (input == null)
			{
				bytes = new byte[] { 0, 0, 0, 0 };
				return false;
			}
			if (TryGetColorBytesARGB(input, out var rgb))
			{
				bytes = rgb;
				return true;
			}
			if (TryGetColorBytesHex(input, out var hex))
			{
				bytes = hex;
				return true;
			}
			bytes = new byte[] { 0, 0, 0, 0 };
			return false;
		}
	}
}