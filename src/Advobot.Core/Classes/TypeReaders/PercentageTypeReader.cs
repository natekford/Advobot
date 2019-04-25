using System;
using System.Threading.Tasks;
using Advobot.Classes.Attributes;
using Discord.Commands;
using ImageMagick;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// Attempts to parse a percentage from a string.
	/// </summary>
	[TypeReaderTargetType(typeof(Percentage))]
	public sealed class PercentageTypeReader : TypeReader
	{
		/// <summary>
		/// Creates a percentage from a string.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			return double.TryParse(input, out var val)
				? Task.FromResult(TypeReaderResult.FromSuccess(new Percentage(val)))
				: Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"Invalid percentage supplied."));
		}
	}
}