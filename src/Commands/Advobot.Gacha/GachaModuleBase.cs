﻿using Advobot.Classes.Modules;
using Advobot.Gacha.Checkers;
using Advobot.Gacha.Database;
using Advobot.Gacha.Displays;
using Advobot.Gacha.Models;
using Advobot.Utilities;
using AdvorangesUtils;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Gacha
{
	public abstract class GachaModuleBase : AdvobotModuleBase
	{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public GachaDatabase Database { get; set; }
		public ICheckersService Checkers { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

		protected async Task<CharacterDisplay> CreateCharacterDisplayAsync(Character character, bool fireAndForget = true)
		{
			var metadata = await Database.GetCharacterMetadataAsync(character).CAF();
			var images = await Database.GetImagesAsync(character).CAF();
			var claims = await Database.GetClaimsAsync(Context.Guild.Id).CAF();
			var claim = claims.SingleOrDefault(x => x.CharacterId == character.CharacterId);
			var display = new CharacterDisplay(Context.Client, Database, metadata, images, claim);
			return await FireAndForget(display, fireAndForget).CAF();
		}
		protected async Task<HaremDisplay> CreateHaremDisplayAsync(User user, bool fireAndForget = true)
		{
			var marriages = await Database.GetClaimsAsync(user).CAF();
			var display = new HaremDisplay(Context.Client, Database, marriages);
			return await FireAndForget(display, fireAndForget).CAF();
		}
		protected async Task<RollDisplay> CreateRollDisplayAsync(bool fireAndForget = true)
		{
			var checker = Checkers.GetClaimChecker(Context.Guild);
			var character = await Database.GetUnclaimedCharacter(Context.Guild.Id).CAF();
			var source = await Database.GetSourceAsync(character.SourceId).CAF();
			var wishes = await Database.GetWishesAsync(Context.Guild.Id, character).CAF();
			var images = await Database.GetImagesAsync(character).CAF();
			var display = new RollDisplay(Context.Client, Database, checker, character, source, wishes, images);
			return await FireAndForget(display, fireAndForget).CAF();
		}
		protected async Task<SourceDisplay> CreateSourceDisplayAsync(Source source, bool fireAndForget = true)
		{
			var characters = await Database.GetCharactersAsync(source).CAF();
			var display = new SourceDisplay(Context.Client, Database, source, characters);
			return await FireAndForget(display, fireAndForget).CAF();
		}
		private async Task<T> FireAndForget<T>(T display, bool fireAndForget) where T : Display
		{
			if (fireAndForget)
			{
				await display.SendAsync(Context.Channel).CAF();
			}
			return display;
		}
	}
}
