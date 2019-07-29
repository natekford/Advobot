using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Advobot.Attributes;
using AdvorangesUtils;
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
		public static IList<string> TrueVals { get; } = new List<string> { "true", "yes", "add", "enable", "set", "positive" };
		/// <summary>
		/// Values that will set the stored bool to false.
		/// </summary>
		public static IList<string> FalseVals { get; } = new List<string> { "false", "no", "remove", "disable", "unset", "negative" };

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
			if (TrueVals.CaseInsContains(input))
			{
				return Task.FromResult(TypeReaderResult.FromSuccess(true));
			}
			else if (FalseVals.CaseInsContains(input))
			{
				return Task.FromResult(TypeReaderResult.FromSuccess(false));
			}
			return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid boolean value provided."));
		}
	}
}