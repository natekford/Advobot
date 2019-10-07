using System;
using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Gacha.Database;
using Advobot.Gacha.ReadOnlyModels;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Gacha.TypeReaders
{
	[TypeReaderTargetType(typeof(IReadOnlySource))]
	public sealed class SourceTypeReader : TypeReader
	{
		public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			var db = services.GetRequiredService<GachaDatabase>();
			return TypeReaderResult.FromSuccess(await db.GetSourceAsync(1).CAF());
		}
	}
}