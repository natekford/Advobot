using YACCS.TypeReaders;

namespace Advobot.TypeReaders;

/// <summary>
/// Attempts to parse permissions.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class PermissionsTypeReader<T>()
	: TryParseTypeReader<T>(TryParse) where T : struct, Enum
{
	private static readonly char[] _ReplaceChars = ['|', '/', '\\'];
	private static readonly char[] _TrimChars = ['"'];

	private static bool TryParse(string s, out T result)
	{
		s = s.Trim(_TrimChars);
		foreach (var replaceChar in _ReplaceChars)
		{
			// EnumSeparatorChar is a const char = ','
			// so I don't think localization matters for this
			s = s.Replace(replaceChar, ',');
		}

		return Enum.TryParse(s, true, out result);
	}
}