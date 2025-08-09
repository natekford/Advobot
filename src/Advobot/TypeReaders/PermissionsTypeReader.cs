using Advobot.Utilities;

using Discord.Commands;

namespace Advobot.TypeReaders;

/// <summary>
/// Attempts to parse permissions.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class PermissionsTypeReader<T> : TypeReader where T : struct, Enum
{
	private static readonly char[] _ReplaceChars = ['|', '/', '\\'];
	private static readonly char[] _TrimChars = ['"'];

	/// <summary>
	/// Checks for valid ulong first, then checks permission names.
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
		input = input.Trim(_TrimChars);
		if (Enum.TryParse<T>(input, true, out var result))
		{
			return TypeReaderResult.FromSuccess(result).AsTask();
		}

		foreach (var replaceChar in _ReplaceChars)
		{
			// TODO: does culture list separator matter?
			input = input.Replace(replaceChar, ',');
		}
		if (Enum.TryParse(input, true, out result))
		{
			return TypeReaderResult.FromSuccess(result).AsTask();
		}

		return TypeReaderUtils.ParseFailedResult<T>().AsTask();
	}
}