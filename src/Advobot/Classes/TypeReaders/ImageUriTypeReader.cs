using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// Attempts to find an image url from the given context.
	/// </summary>
	public sealed class UriTypeReader : TypeReader
	{
		/// <summary>
		/// Checks if the input is a valid uri.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			if (!Uri.TryCreate(input, UriKind.Absolute, out var url))
			{
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid url provided in message content."));
			}
			return url != null
				? Task.FromResult(TypeReaderResult.FromSuccess(url))
				: Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, "No valid url found."));
		}
	}
}
