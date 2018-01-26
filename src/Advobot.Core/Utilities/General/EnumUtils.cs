using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Advobot.Core.Utilities
{
    public static class EnumUtils
    {
		/// <summary>
		/// Converts an enum to the names of the values it contains.
		/// </summary>
		/// <param name="value">The instance value of the enum.</param>
		/// <returns>The names of the values <paramref name="value"/> contains.</returns>
		/// <exception cref="ArgumentNullException">When <paramref name="value"/> is null.</exception>
		public static IEnumerable<string> GetNamesFromEnum(Enum value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("Value must not be null.", nameof(value));
			}

			//If the enum ToString is different from its value ToString then that means it's a valid flags enum
			if (value.ToString() != Convert.ChangeType(value, value.GetTypeCode()).ToString())
			{
				foreach (var name in value.ToString().Split(',', '|').Select(x => x.Trim()))
				{
					yield return name;
				}
				yield break;
			}

			//Loop through every possible flag from the enum's values to see which ones match
			var type = value.GetType();
			foreach (Enum e in Enum.GetValues(type))
			{
				if (value.HasFlag(e))
				{
					yield return Enum.GetName(type, e);
				}
			}
		}
		public static IEnumerable<TEnum> GetEnumsFromEnum<TEnum>(TEnum value) where TEnum : struct, IComparable, IConvertible, IFormattable
		{
			if (!(value is Enum enumValue))
			{
				throw new ArgumentException("Invalid generic parameter type. Must be an enum.", nameof(TEnum));
			}

			//Loop through every possible flag from the enum's values to see which ones match
			var type = value.GetType();
			foreach (Enum e in Enum.GetValues(type))
			{
				if (enumValue.HasFlag(e))
				{
					yield return (TEnum)(object)e;
				}
			}
		}

		/// <summary>
		/// Attempts to parse enums from the supplied values. <typeparamref name="TEnum"/> MUST be an enum.
		/// </summary>
		/// <typeparam name="TEnum">The enum to parse.</typeparam>
		/// <param name="input">The input names.</param>
		/// <param name="value">The valid enums.</param>
		/// <param name="invalidInput">The invalid names.</param>
		/// <returns>A boolean indicating if there were any failed parses.</returns>
		/// <exception cref="ArgumentException">When <typeparamref name="TEnum"/> is not an enum.</exception>
		public static bool TryParseEnums<TEnum>(IEnumerable<string> input, out List<TEnum> validInput, out List<string> invalidInput) where TEnum : struct, IComparable, IConvertible, IFormattable
		{
			if (!typeof(TEnum).IsEnum)
			{
				throw new ArgumentException("Invalid generic parameter type. Must be an enum.", nameof(TEnum));
			}

			validInput = new List<TEnum>();
			invalidInput = new List<string>();
			foreach (var enumName in input)
			{
				if (Enum.TryParse<TEnum>(enumName, true, out var result))
				{
					validInput.Add(result);
				}
				else
				{
					invalidInput.Add(enumName);
				}
			}
			return !invalidInput.Any();
		}
		/// <summary>
		/// Attempts to parse all enums then OR them together. <typeparamref name="TEnum"/> MUST be an enum.
		/// </summary>
		/// <typeparam name="TEnum">The enum to parse.</typeparam>
		/// <param name="input">The input names.</param>
		/// <param name="value">The return value of every valid enum ORed together.</param>
		/// <param name="invalidInput">The invalid names.</param>
		/// <returns>A boolean indicating if there were any failed parses.</returns>
		/// <exception cref="ArgumentException">When <typeparamref name="TEnum"/> is not an enum.</exception>
		public static bool TryParseEnums<TEnum>(IEnumerable<string> input, out TEnum value, out List<string> invalidInput) where TEnum : struct, IComparable, IConvertible, IFormattable
		{
			var enumType = typeof(TEnum);
			if (!enumType.IsEnum)
			{
				throw new ArgumentException($"Invalid generic parameter type. Must be an enum.", nameof(TEnum));
			}

			var temp = new TEnum();
			invalidInput = new List<string>();
			foreach (var enumName in input)
			{
				if (Enum.TryParse(enumName, true, out TEnum result))
				{
					//Cast as dynamic so bitwise functions can be done on it
					temp |= (dynamic)result;
				}
				else
				{
					invalidInput.Add(enumName);
				}
			}
			value = (TEnum)Enum.Parse(enumType, temp.ToString());
			return !invalidInput.Any();
		}

		internal static bool TryParseEnums2<TEnum>(IEnumerable<string> input, out TEnum value, out List<string> invalidInput) where TEnum : struct, IComparable, IConvertible, IFormattable
		{
			var enumType = typeof(TEnum);
			if (!enumType.IsEnum)
			{
				throw new ArgumentException($"Invalid generic parameter type. Must be an enum.", nameof(TEnum));
			}

			invalidInput = new List<string>();
			switch (Type.GetTypeCode(enumType))
			{
				case TypeCode.Byte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					ulong unsigned = 0;
					foreach (var enumName in input)
					{
						if (!Enum.TryParse(enumName, true, out TEnum result))
						{
							invalidInput.Add(enumName);
							continue;
						}
						unsigned |= Convert.ToUInt64(result);
					}
					value = (TEnum)Enum.Parse(enumType, unsigned.ToString());
					return !invalidInput.Any();
				default:
					long signed = 0;
					foreach (var enumName in input)
					{
						if (!Enum.TryParse(enumName, true, out TEnum result))
						{
							invalidInput.Add(enumName);
							continue;
						}
						signed |= Convert.ToInt64(result);
					}
					value = (TEnum)Enum.Parse(enumType, signed.ToString());
					return !invalidInput.Any();
			}
		}
	}
}
