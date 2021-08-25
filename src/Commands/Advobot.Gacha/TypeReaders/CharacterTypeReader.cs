
using Advobot.Attributes;
using Advobot.Gacha.Database;
using Advobot.Gacha.Models;
using Advobot.Gacha.Utilities;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Gacha.TypeReaders
{
	[TypeReaderTargetType(typeof(Character))]
	public sealed class CharacterTypeReader : TypeReader
	{
		public override async Task<TypeReaderResult> ReadAsync(
			ICommandContext context,
			string input,
			IServiceProvider services)
		{
			var db = services.GetRequiredService<IGachaDatabase>();
			var characters = await db.GetCharactersAsync(input).CAF();
			return TypeReaderUtils.SingleValidResult(characters, "characters", input);
		}
	}
}