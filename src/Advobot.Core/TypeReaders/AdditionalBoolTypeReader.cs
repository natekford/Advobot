using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Advobot.Attributes;
using Discord.Commands;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// Attempts to parse bools and also other positive/negative words.
	/// </summary>
	[TypeReaderTargetType(typeof(bool))]
	public sealed class AdditionalBoolTypeReader : TypeReader
	{
		/// <summary>
		/// Values that will set the stored bool to true.
		/// </summary>
		public static readonly ImmutableHashSet<string> TrueVals = new[]
		{
			"true",
			"yes",
			"add",
			"enable",
			"set",
			"positive"
		}.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
		/// <summary>
		/// Values that will set the stored bool to false.
		/// </summary>
		public static readonly ImmutableHashSet<string> FalseVals = new[]
		{
			"false",
			"no",
			"remove",
			"disable",
			"unset",
			"negative"
		}.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Converts a string into a true bool if it has a match in <see cref="TrueVals"/>, 
		/// false bool if it has a match in <see cref="FalseVals"/>, 
		/// or returns an error.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			if (TrueVals.Contains(input))
			{
				return Task.FromResult(TypeReaderResult.FromSuccess(true));
			}
			else if (FalseVals.Contains(input))
			{
				return Task.FromResult(TypeReaderResult.FromSuccess(false));
			}
			return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid boolean value provided."));
		}
	}
}