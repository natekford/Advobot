using Discord.Commands;
using System;
using Advobot.Classes.Attributes;
using Advobot.Gacha.Models;
using System.Threading.Tasks;

namespace Advobot.Gacha.TypeReaders
{
	[TypeReaderTargetType(typeof(Character))]
	public sealed class CharacterTypeReader : TypeReader
	{
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			throw new NotImplementedException();
		}
	}
}
