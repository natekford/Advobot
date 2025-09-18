using Advobot.Modules;

using YACCS.TypeReaders;

namespace Advobot.TypeReaders.Discord;

/// <summary>
/// Type reader for Discord items (users, roles, bans, etc).
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class DiscordTypeReader<T> : TypeReader<IGuildContext, T>
{
	/// <summary>
	/// Returns success if only one object, returns errors if zero or multiple.
	/// </summary>
	/// <param name="matches"></param>
	/// <returns></returns>
	protected static ITypeReaderResult<T> SingleValidResult(IReadOnlyCollection<T> matches)
	{
		if (matches.Count == 1)
		{
			return TypeReaderResult<T>.Success(matches.Single());
		}
		else if (matches.Count > 1)
		{
			return TypeReaderResult<T>.TooManyMatches.Result;
		}
		return TypeReaderResult<T>.ParseFailed.Result;
	}
}