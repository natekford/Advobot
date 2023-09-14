using Advobot.Gacha.Counters;
using Advobot.Gacha.Database;
using Advobot.Gacha.Interaction;
using Advobot.Gacha.Models;
using Advobot.Services.Time;

using AdvorangesUtils;

using Discord;
using Discord.WebSocket;

using System.Collections.Concurrent;

namespace Advobot.Gacha.Displays;

public sealed class DisplayManager(
	IGachaDatabase db,
	BaseSocketClient client,
	ICounterService counters,
	IInteractionManager interaction,
	ITime time)
{
	private readonly BaseSocketClient _Client = client;
	private readonly ICounterService _Counters = counters;
	private readonly IGachaDatabase _Db = db;

	private readonly ConcurrentDictionary<ulong, int> _Ids = new();
	private readonly IInteractionManager _Interaction = interaction;
	private readonly ITime _Time = time;

	public async Task<CharacterDisplay> CreateCharacterDisplayAsync(IGuild guild, Character character)
	{
		var id = GetDisplayId(guild);
		var metadata = await _Db.GetCharacterMetadataAsync(character).CAF();
		var images = await _Db.GetImagesAsync(character).CAF();
		var claim = await _Db.GetClaimAsync(guild.Id, character).CAF();
		return new(_Db, _Time, _Interaction, _Client, id, metadata, images, claim);
	}

	public async Task<HaremDisplay> CreateHaremDisplayAsync(IGuild guild, User user)
	{
		var id = GetDisplayId(guild);
		var marriages = await _Db.GetClaimsAsync(user).CAF();
		return new(_Db, _Time, _Interaction, id, marriages);
	}

	public async Task<RollDisplay> CreateRollDisplayAsync(IGuild guild)
	{
		var id = GetDisplayId(guild);
		var checker = _Counters.GetClaims(guild);
		var character = await _Db.GetUnclaimedCharacter(guild.Id).CAF();
		var source = await _Db.GetSourceAsync(character.SourceId).CAF();
		var wishes = await _Db.GetWishesAsync(guild.Id, character).CAF();
		var images = await _Db.GetImagesAsync(character).CAF();
		return new(_Db, _Time, _Interaction, id, checker, character, source, wishes, images);
	}

	public async Task<SourceDisplay> CreateSourceDisplayAsync(IGuild guild, Source source)
	{
		var id = GetDisplayId(guild);
		var characters = await _Db.GetCharactersAsync(source).CAF();
		return new(_Db, _Time, _Interaction, id, source, characters);
	}

	private int GetDisplayId(IGuild guild)
		=> _Ids.AddOrUpdate(guild.Id, 1, (_, value) => value + 1);
}