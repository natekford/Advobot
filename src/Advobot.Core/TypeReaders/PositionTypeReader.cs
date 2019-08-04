using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// Parses something from a position.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class PositionTypeReader<T> : TypeReader
	{
		/// <summary>
		/// The name of the type of objects being found. This should be pluralized.
		/// </summary>
		public abstract string ObjectTypeName { get; }

		/// <inheritdoc />
		public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			if (!int.TryParse(input, out var position))
			{
				return TypeReaderUtils.ParseFailedResult<int>();
			}

			var matches = (await GetObjectsWithPositionAsync(context, position).CAF()).ToArray();
			return TypeReaderUtils.SingleValidResult(matches, $"{ObjectTypeName} by position", input);
		}
		/// <summary>
		/// Gets objects with the supplied position.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		protected abstract Task<IEnumerable<T>> GetObjectsWithPositionAsync(ICommandContext context, int position);
	}
}