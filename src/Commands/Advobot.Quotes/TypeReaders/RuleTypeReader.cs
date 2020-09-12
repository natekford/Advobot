using System;
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
	[TypeReaderTargetType(typeof(IReadOnlyRule))]
	public sealed class RuleTypeReader : TypeReader
	{
		public override async Task<TypeReaderResult> ReadAsync(
			ICommandContext context,
			string input,
			IServiceProvider services)
		{
			var split = input.Split('.', StringSplitOptions.RemoveEmptyEntries);
			if (split.Length != 2
				|| !int.TryParse(split[0], out var category)
				|| !int.TryParse(split[1], out var position))
			{
				return TypeReaderUtils.ParseFailedResult<IReadOnlyRule>();
			}

			var db = services.GetRequiredService<RuleDatabase>();
			var rule = await db.GetRuleAsync(context.Guild.Id, category, position).CAF();
			if (rule is null)
			{
				return TypeReaderUtils.ParseFailedResult<IReadOnlyRule>();
			}
			return TypeReaderResult.FromSuccess(rule);
		}
	}
}