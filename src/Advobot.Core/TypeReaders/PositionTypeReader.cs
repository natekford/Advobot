using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
		/// The type to find from a position.
		/// </summary>
		public abstract string ObjectType { get; }

		/// <inheritdoc />
		public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			if (!int.TryParse(input, out var position))
			{
				return TypeReaderResult.FromError(CommandError.ParseFailed, "Failed to parse the position.");
			}

			var samePos = await GetObjectsWithPosition(context, position).CAF();
			if (samePos.Count == 1)
			{
				return TypeReaderResult.FromSuccess(samePos.Single());
			}
			if (samePos.Count > 1)
			{
				return TypeReaderResult.FromError(CommandError.MultipleMatches, $"Multiple {ObjectType}s have the supplied position.");
			}
			return TypeReaderResult.FromError(CommandError.ObjectNotFound, $"There is no {ObjectType} with the supplied position.");
		}
		/// <summary>
		/// Gets objects with the supplied position.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		public abstract Task<IReadOnlyCollection<T>> GetObjectsWithPosition(ICommandContext context, int position);
	}
}