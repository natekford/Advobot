using System;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Quotes.Database;
using Advobot.Quotes.ReadOnlyModels;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.TypeReaders
{
	[TypeReaderTargetType(typeof(IReadOnlyRuleCategory))]
	public sealed class RuleCategoryTypeReader : TypeReader
	{
		public override async Task<TypeReaderResult> ReadAsync(
			ICommandContext context,
			string input,
			IServiceProvider services)
		{
			if (!int.TryParse(input, out var category))
			{
				return TypeReaderUtils.ParseFailedResult<IReadOnlyRuleCategory>();
			}

			var db = services.GetRequiredService<RuleDatabase>();
			var categories = await db.GetCategoriesAsync(context.Guild.Id).CAF();
			var matches = categories.Where(x => x.Category == category).ToArray();
			return TypeReaderUtils.SingleValidResult(matches, "categories", input);
		}
	}
}