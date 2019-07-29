using Discord.Commands;
using System;
using Advobot.Attributes;
using Advobot.Gacha.Models;
using System.Threading.Tasks;
using AdvorangesUtils;
using Advobot.Gacha.Database;
using Microsoft.Extensions.DependencyInjection;
using Advobot.Utilities;

namespace Advobot.Gacha.TypeReaders
{
	[TypeReaderTargetType(typeof(Character))]
	public sealed class CharacterTypeReader : TypeReader
	{
		public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			var db = services.GetRequiredService<GachaDatabase>();
			var id = int.Parse(input);
			return TypeReaderResult.FromSuccess(await db.GetCharacterAsync(id).CAF());
		}
	}
}
