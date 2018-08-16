using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes.Settings;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// Attempts to find a quote with the supplied name.
	/// </summary>
	public sealed class QuoteTypeReader : TypeReader
	{
		/// <summary>
		/// Attempts to find a quote with the supplied input as a name.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			return context is AdvobotCommandContext aContext
					&& aContext.GuildSettings.Quotes.SingleOrDefault(x => x.Name.CaseInsEquals(input)) is Quote quote
				? Task.FromResult(TypeReaderResult.FromSuccess(quote))
				: Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, $"Unable to find a quote matching `{input}`."));
		}
	}
}