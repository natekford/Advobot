using Advobot.Attributes;
using Advobot.CloseWords;
using Advobot.Quotes.Database;
using Advobot.Quotes.Models;
using Advobot.Utilities;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Quotes.TypeReaders;

[TypeReaderTargetType(typeof(IReadOnlyList<Quote>))]
public sealed class CloseQuoteTypeReader : TypeReader
{
	public override async Task<TypeReaderResult> ReadAsync(
		ICommandContext context,
		string input,
		IServiceProvider services)
	{
		var db = services.GetRequiredService<IQuoteDatabase>();
		var quotes = await db.GetQuotesAsync(context.Guild.Id).ConfigureAwait(false);
		var matches = new CloseWords<Quote>(quotes, x => x.Name)
			.FindMatches(input)
			.Select(x => x.Value)
			.ToArray();
		return TypeReaderUtils.MultipleValidResults(matches, "quotes", input);
	}
}