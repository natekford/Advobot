using Advobot.Attributes;
using Advobot.Quotes.Database;
using Advobot.Quotes.Models;
using Advobot.Utilities;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Quotes.TypeReaders;

[TypeReaderTargetType(typeof(RuleCategory))]
public sealed class RuleCategoryTypeReader : TypeReader
{
	public override async Task<TypeReaderResult> ReadAsync(
		ICommandContext context,
		string input,
		IServiceProvider services)
	{
		if (!int.TryParse(input, out var category))
		{
			return TypeReaderUtils.ParseFailedResult<RuleCategory>();
		}

		var db = services.GetRequiredService<RuleDatabase>();
		var categories = await db.GetCategoriesAsync(context.Guild.Id).ConfigureAwait(false);
		var matches = categories.Where(x => x.Category == category).ToArray();
		return TypeReaderUtils.SingleValidResult(matches, "categories", input);
	}
}