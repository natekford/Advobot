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
		// TODO: span?
		s = s.Trim(_TrimChars);
		foreach (var replaceChar in _ReplaceChars)
		{
			// TODO: does culture list separator matter?
			s = s.Replace(replaceChar, ',');
		}

		return Enum.TryParse(s, true, out result);
	}
}