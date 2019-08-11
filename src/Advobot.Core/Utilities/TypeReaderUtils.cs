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
		/// Acts as <see cref="FromSuccess(TypeReader, object)"/> but async.
		/// </summary>
		/// <param name="_"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Task<TypeReaderResult> FromSuccessAsync(
			this TypeReader _,
			object value)
			=> Task.FromResult(FromSuccess(_, value));
		/// <summary>
		/// Returns success with the given object.
		/// </summary>
		/// <param name="_"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TypeReaderResult FromSuccess(
			this TypeReader _,
			object value)
			=> TypeReaderResult.FromSuccess(value);
		/// <summary>
		/// Acts as <see cref="SingleValidResult{T}(TypeReader, IReadOnlyList{T}, string, string)"/> but async.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="_"></param>
		/// <param name="matches"></param>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Task<TypeReaderResult> SingleValidResultAsync<T>(
			this TypeReader _,
			IReadOnlyList<T> matches,
			string type,
			string value)
			=> Task.FromResult(SingleValidResult(_, matches, type, value));
		/// <summary>
		/// Returns success if only one object, returns errors if zero or multiple.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="_"></param>
		/// <param name="matches"></param>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TypeReaderResult SingleValidResult<T>(
			this TypeReader _,
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
		/// Acts as <see cref="MultipleValidResults{T}(TypeReader, IReadOnlyList{T}, string, string)"/> but async.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="_"></param>
		/// <param name="matches"></param>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Task<TypeReaderResult> MultipleValidResultsAsync<T>(
			this TypeReader _,
			IReadOnlyList<T> matches,
			string type,
			string value)
			=> Task.FromResult(MultipleValidResults(_, matches, type, value));
		/// <summary>
		/// Returns success if at least one object, returns error if multiple.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="_"></param>
		/// <param name="matches"></param>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TypeReaderResult MultipleValidResults<T>(
			this TypeReader _,
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
		/// Acts as <see cref="ParseFailedResult{T}"/> but async.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="_"></param>
		/// <returns></returns>
		public static Task<TypeReaderResult> ParseFailedResultAsync<T>(
			this TypeReader _)
			=> Task.FromResult(ParseFailedResult<T>(_));
		/// <summary>
		/// Returns a string saying 'Failed to parse <typeparamref name="T"/>'.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="_"></param>
		/// <returns></returns>
		public static TypeReaderResult ParseFailedResult<T>(
			this TypeReader _)
			=> TypeReaderResult.FromError(CommandError.ParseFailed, $"Failed to parse {typeof(T).Name}.");
	}
}
