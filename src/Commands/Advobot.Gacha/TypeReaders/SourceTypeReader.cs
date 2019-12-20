using System;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Gacha.Database;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Gacha.TypeReaders
{
	[TypeReaderTargetType(typeof(IReadOnlySource))]
	public sealed class SourceTypeReader : TypeReader
	{
		public override async Task<TypeReaderResult> ReadAsync(
			ICommandContext context,
			string input,
			IServiceProvider services)
		{
			var db = services.GetRequiredService<IGachaDatabase>();
			var matches = db.SourceIds.FindMatches(input);
			var sources = await db.GetSourcesAsync(matches.Select(x => x.Value.Id)).CAF();
			return TypeReaderUtils.SingleValidResult(sources, "sources", input);
		}
	}
}