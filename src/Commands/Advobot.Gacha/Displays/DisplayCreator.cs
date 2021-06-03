using System.Collections.Concurrent;
using System.Threading.Tasks;

using Advobot.Gacha.Counters;
using Advobot.Gacha.Database;
using Advobot.Gacha.Interaction;
using Advobot.Gacha.Models;
using Advobot.Services.Time;

using AdvorangesUtils;

using Discord;
using Discord.WebSocket;

namespace Advobot.Gacha.Displays
{
	public sealed class DisplayManager
	{
		private readonly BaseSocketClient _Client;
		private readonly ICounterService _Counters;
		private readonly IGachaDatabase _Db;

		private readonly ConcurrentDictionary<ulong, int> _Ids = new();
		private readonly IInteractionManager _Interaction;
		private readonly ITime _Time;

		public DisplayManager(
			IGachaDatabase db,
			BaseSocketClient client,
			ICounterService counters,
			IInteractionManager interaction,
			ITime time)
		{
			_Db = db;
			_Client = client;
			_Counters = counters;
			_Interaction = interaction;
			_Time = time;
		}

		public async Task<Display> CreateCharacterDisplayAsync(IGuild guild, Character character)
		{
			var id = GetDisplayId(guild);
			var metadata = await _Db.GetCharacterMetadataAsync(character).CAF();
			var images = await _Db.GetImagesAsync(character).CAF();
			var claim = await _Db.GetClaimAsync(guild.Id, character).CAF();
			return new CharacterDisplay(_Db, _Time, _Interaction, _Client, id, metadata, images, claim);
		}

		public async Task<Display> CreateHaremDisplayAsync(IGuild guild, User user)
		{
			var id = GetDisplayId(guild);
			var marriages = await _Db.GetClaimsAsync(user).CAF();
			return new HaremDisplay(_Db, _Time, _Interaction, id, marriages);
		}

		public async Task<Display> CreateRollDisplayAsync(IGuild guild)
		{
			var id = GetDisplayId(guild);
			var checker = _Counters.GetClaims(guild);
			var character = await _Db.GetUnclaimedCharacter(guild.Id).CAF();
			var source = await _Db.GetSourceAsync(character.SourceId).CAF();
			var wishes = await _Db.GetWishesAsync(guild.Id, character).CAF();
			var images = await _Db.GetImagesAsync(character).CAF();
			return new RollDisplay(_Db, _Time, _Interaction, id, checker, character, source, wishes, images);
		}

		public async Task<Display> CreateSourceDisplayAsync(IGuild guild, Source source)
		{
			var id = GetDisplayId(guild);
			var characters = await _Db.GetCharactersAsync(source).CAF();
			return new SourceDisplay(_Db, _Time, _Interaction, id, source, characters);
		}

		private int GetDisplayId(IGuild guild)
			=> _Ids.AddOrUpdate(guild.Id, 1, (_, value) => value + 1);
	}
}