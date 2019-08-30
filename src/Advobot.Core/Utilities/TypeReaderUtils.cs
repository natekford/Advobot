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
		/// Returns success with the given object.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TypeReaderResult FromSuccess(object? value)
			=> TypeReaderResult.FromSuccess(value);

		/// <summary>
		/// Acts as <see cref="FromSuccess(object)"/> but async.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Task<TypeReaderResult> FromSuccessAsync(object? value)
			=> Task.FromResult(FromSuccess(value));

		/// <summary>
		/// Returns success if at least one object, returns error if multiple.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="matches"></param>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TypeReaderResult MultipleValidResults<T>(
			IReadOnlyList<T> matches,
			string type,
			string value)
		{
			if (matches.Count > 0)
			{
				return TypeReaderResult.FromSuccess(matches);
			}
			var noneError = $"Unable to find any {type} matching `{value}`.";
			return TypeReaderResult.FromError(CommandError.ObjectNotFound, noneError);
		}

		/// <summary>
		/// Acts as <see cref="MultipleValidResults{T}(IReadOnlyList{T}, string, string)"/> but async.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="matches"></param>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Task<TypeReaderResult> MultipleValidResultsAsync<T>(
			IReadOnlyList<T> matches,
			string type,
			string value)
			=> Task.FromResult(MultipleValidResults(matches, type, value));

		/// <summary>
		/// Returns a string saying 'Failed to parse <typeparamref name="T"/>'.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static TypeReaderResult ParseFailedResult<T>()
			=> TypeReaderResult.FromError(CommandError.ParseFailed, $"Failed to parse {typeof(T).Name}.");

		/// <summary>
		/// Acts as <see cref="ParseFailedResult{T}"/> but async.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static Task<TypeReaderResult> ParseFailedResultAsync<T>()
			=> Task.FromResult(ParseFailedResult<T>());

		/// <summary>
		/// Returns success if only one object, returns errors if zero or multiple.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="matches"></param>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TypeReaderResult SingleValidResult<T>(
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
		/// Acts as <see cref="SingleValidResult{T}(IReadOnlyList{T}, string, string)"/> but async.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="matches"></param>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Task<TypeReaderResult> SingleValidResultAsync<T>(
			IReadOnlyList<T> matches,
			string type,
			string value)
			=> Task.FromResult(SingleValidResult(matches, type, value));
	}
}