﻿using Advobot.Attributes;
using Advobot.Quotes.Database;
using Advobot.Quotes.Models;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.TypeReaders;

[TypeReaderTargetType(typeof(Quote))]
public sealed class QuoteTypeReader : TypeReader
{
	public override async Task<TypeReaderResult> ReadAsync(
		ICommandContext context,
		string input,
		IServiceProvider services)
	{
		var db = services.GetRequiredService<IQuoteDatabase>();
		var quotes = await db.GetQuotesAsync(context.Guild.Id).CAF();
		var matches = quotes.Where(x => x.Name.CaseInsEquals(input)).ToArray();
		return TypeReaderUtils.SingleValidResult(matches, "quotes", input);
	}
}