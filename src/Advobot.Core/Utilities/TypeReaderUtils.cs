using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;

namespace Advobot.Utilities
{
	/// <summary>
	/// Utilities for returning <see cref="TypeReader"/> results.
	/// </summary>
	public static class TypeReaderUtils
	{
		/// <summary>
		/// Acts as <see cref="MatchesResult{T}(IReadOnlyList{T}, string, string)"/> but async.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="matches"></param>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Task<TypeReaderResult> MatchesResultAsync<T>(
			IReadOnlyList<T> matches,
			string type,
			string value)
			=> Task.FromResult(MatchesResult(matches, type, value));
		/// <summary>
		/// Returns success if only one object, returns errors if zero or multiple.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="matches"></param>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TypeReaderResult MatchesResult<T>(
			IReadOnlyList<T> matches,
			string type,
			string value)
		{
			if (matches.Count == 1)
			{
				return TypeReaderResult.FromSuccess(matches[0]);
			}
			else if (matches.Count > 1)
			{
				var tooManyError = $"{matches.Count} {type} match `{value}`.";
				return TypeReaderResult.FromError(CommandError.MultipleMatches, tooManyError);
			}
			var noneError = $"Unable to find any {type} matching `{value}`.";
			return TypeReaderResult.FromError(CommandError.ObjectNotFound, noneError);
		}
		/// <summary>
		/// Acts as <see cref="ParseFailedResult{T}"/> but async.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static Task<TypeReaderResult> ParseFailedResultAsync<T>()
			=> Task.FromResult(ParseFailedResult<T>());
		/// <summary>
		/// Returns a string saying 'Failed to parse <typeparamref name="T"/>'.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static TypeReaderResult ParseFailedResult<T>()
			=> TypeReaderResult.FromError(CommandError.ParseFailed, $"Failed to parse {typeof(T).Name}.");
	}
}
