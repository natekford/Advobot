using System;
using System.Globalization;

namespace Advobot.SharedUI
{
	/// <summary>
	/// UI utilities shared between .Net Core and .Net Framework UIs.
	/// </summary>
	public class SharedUIUtils
	{
		/// <summary>
		/// Gets ARGB color bytes.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static byte[] ParseColorBytes(string input)
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
		/// <summary>
		/// Attempts to get ARGB color bytes.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static bool TryParseColorBytes(string input, out byte[] bytes)
		{
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
			bytes = default;
			return false;
		}
		private static bool TryGetColorBytesARGB(string input, out byte[] bytes)
		{
			var split = input.Split('/');
			if (split.Length < 3 || split.Length > 4)
			{
				bytes = default;
				return false;
			}

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
					bytes = default;
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
				if (BitConverter.IsLittleEndian)
				{
					Array.Reverse(bytes);
				}
				return true;
			}
			bytes = default;
			return false;
		}
	}
}