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
		/// Creates a <see cref="Task{T}"/> returning <paramref name="result"/>.
		/// </summary>
		/// <param name="result"></param>
		/// <returns></returns>
		public static Task<TypeReaderResult> AsTask(this TypeReaderResult result)
			=> Task.FromResult(result);

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
		/// Returns failure indicating an object was not found.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TypeReaderResult NotFoundResult(string type, string value)
		{
			var noneError = $"Unable to find any {type} matching `{value}`.";
			return TypeReaderResult.FromError(CommandError.ObjectNotFound, noneError);
		}

		/// <summary>
		/// Returns a string saying 'Failed to parse <typeparamref name="T"/>'.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static TypeReaderResult ParseFailedResult<T>()
			=> TypeReaderResult.FromError(CommandError.ParseFailed, $"Failed to parse {typeof(T).Name}.");

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
			return NotFoundResult(type, value);
		}
	}
}