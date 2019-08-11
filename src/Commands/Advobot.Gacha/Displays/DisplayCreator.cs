using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Advobot.Gacha.Checkers;
using Advobot.Gacha.Database;
using Advobot.Gacha.Models;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Gacha.Displays
{
	public sealed class DisplayManager
	{
		private readonly GachaDatabase _Db;
		private readonly ICheckersService _Checkers;
		private readonly ConcurrentDictionary<ulong, int> _DisplayIds
			= new ConcurrentDictionary<ulong, int>();

		public DisplayManager(IServiceProvider services)
		{
			_Db = services.GetRequiredService<GachaDatabase>();
			_Checkers = services.GetRequiredService<ICheckersService>();
		}

		public async Task<Display> CreateRollDisplayAsync(SocketCommandContext context)
		{
			var id = GetDisplayId(context.Guild);
			var checker = _Checkers.GetClaimChecker(context.Guild);
			var character = await _Db.GetUnclaimedCharacter(context.Guild.Id).CAF();
			var source = await _Db.GetSourceAsync(character.SourceId).CAF();
			var wishes = await _Db.GetWishesAsync(context.Guild.Id, character).CAF();
			var images = await _Db.GetImagesAsync(character).CAF();
			return new RollDisplay(context.Client, _Db, id, checker, character, source, wishes, images);
		}
		public async Task<Display> CreateCharacterDisplayAsync(SocketCommandContext context, Character character)
		{
			var id = GetDisplayId(context.Guild);
			var metadata = await _Db.GetCharacterMetadataAsync(character).CAF();
			var images = await _Db.GetImagesAsync(character).CAF();
			var claim = await _Db.GetClaimAsync(context.Guild.Id, character).CAF();
			return new CharacterDisplay(context.Client, _Db, id, metadata, images, claim);
		}
		public async Task<Display> CreateSourceDisplayAsync(SocketCommandContext context, Source source)
		{
			var id = GetDisplayId(context.Guild);
			var characters = await _Db.GetCharactersAsync(source).CAF();
			return new SourceDisplay(context.Client, _Db, id, source, characters);
		}
		public async Task<Display> CreateHaremDisplayAsync(SocketCommandContext context, User user)
		{
			var id = GetDisplayId(context.Guild);
			var marriages = await _Db.GetClaimsAsync(user).CAF();
			return new HaremDisplay(context.Client, _Db, id, marriages);
		}

		private int GetDisplayId(IGuild guild)
			=> _DisplayIds.AddOrUpdate(guild.Id, 1, (key, value) => value + 1);
	}
}
