﻿using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using Advobot.Gacha.Counters;
using Advobot.Gacha.Database;
using Advobot.Gacha.ReadOnlyModels;

using AdvorangesUtils;

using Discord;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Gacha.Displays
{
	public sealed class DisplayManager
	{
		private readonly ICounterService _Checkers;
		private readonly GachaDatabase _Db;

		private readonly ConcurrentDictionary<ulong, int> _Ids
			= new ConcurrentDictionary<ulong, int>();

		private readonly IServiceProvider _Services;

		public DisplayManager(IServiceProvider services)
		{
			_Services = services;
			_Db = services.GetRequiredService<GachaDatabase>();
			_Checkers = services.GetRequiredService<ICounterService>();
		}

		public async Task<Display> CreateCharacterDisplayAsync(IGuild guild, IReadOnlyCharacter character)
		{
			var id = GetDisplayId(guild);
			var metadata = await _Db.GetCharacterMetadataAsync(character).CAF();
			var images = await _Db.GetImagesAsync(character).CAF();
			var claim = await _Db.GetClaimAsync(guild.Id, character).CAF();
			return new CharacterDisplay(_Services, id, metadata, images, claim);
		}

		public async Task<Display> CreateHaremDisplayAsync(IGuild guild, IReadOnlyUser user)
		{
			var id = GetDisplayId(guild);
			var marriages = await _Db.GetClaimsAsync(user).CAF();
			return new HaremDisplay(_Services, id, marriages);
		}

		public async Task<Display> CreateRollDisplayAsync(IGuild guild)
		{
			var id = GetDisplayId(guild);
			var checker = _Checkers.GetClaims(guild);
			var character = await _Db.GetUnclaimedCharacter(guild.Id).CAF();
			var source = await _Db.GetSourceAsync(character.SourceId).CAF();
			var wishes = await _Db.GetWishesAsync(guild.Id, character).CAF();
			var images = await _Db.GetImagesAsync(character).CAF();
			return new RollDisplay(_Services, id, checker, character, source, wishes, images);
		}

		public async Task<Display> CreateSourceDisplayAsync(IGuild guild, IReadOnlySource source)
		{
			var id = GetDisplayId(guild);
			var characters = await _Db.GetCharactersAsync(source).CAF();
			return new SourceDisplay(_Services, id, source, characters);
		}

		private int GetDisplayId(IGuild guild)
			=> _Ids.AddOrUpdate(guild.Id, 1, (_, value) => value + 1);
	}
}