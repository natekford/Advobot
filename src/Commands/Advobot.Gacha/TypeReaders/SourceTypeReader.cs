using Discord.Commands;
using System;
using Advobot.Classes.Attributes;
using Advobot.Gacha.Models;
using System.Threading.Tasks;
using AdvorangesUtils;
using Advobot.Gacha.Database;
using Microsoft.Extensions.DependencyInjection;
using Advobot.Utilities;

namespace Advobot.Gacha.TypeReaders
{
	[TypeReaderTargetType(typeof(Source))]
	public sealed class SourceTypeReader : TypeReader
	{
		public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			var db = services.GetRequiredService<GachaDatabase>();
			return TypeReaderResult.FromSuccess(await db.GetSourceAsync(1).CAF());
		}
	}
}
