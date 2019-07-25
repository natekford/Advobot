using Advobot.Gacha.Metadata;
using Advobot.Gacha.Models;
using Advobot.Gacha.Utils;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Advobot.Gacha.Database
{
	public sealed class GachaDatabase
	{
		private readonly DbContextOptions _Options;

		public GachaDatabase(IServiceProvider provider)
		{
			_Options = provider.GetRequiredService<DbContextOptions>();
		}

		public async Task AddAndSaveAsync<T>(T value) where T : class
		{
			using var context = new GachaContext(_Options);

			var set = context.Set<T>();
			await set.AddAsync(value).CAF();
			await context.SaveChangesAsync().CAF();
		}
		public async Task UpdateAsync<T>(T value) where T : class
		{
			using var context = new GachaContext(_Options);

			var set = context.Set<T>();
			set.Update(value);
			await context.SaveChangesAsync().CAF();
		}
		public async Task UpdateAsync<T, TProperty>(
			T entity,
			Expression<Func<T, TProperty>> propertyExpression,
			TProperty value) where T : class
		{
			using var context = new GachaContext(_Options);

			context.Attach(entity);
			context.Entry(entity).Property(propertyExpression).CurrentValue = value;
			await context.SaveChangesAsync().CAF();
		}

		public Task<Character> GetRandomCharacterAsync(ulong guildId)
		{
			using var context = new GachaContext(_Options);

			var untaken = context.Characters.Where(c => !context.Marriages.Any(m =>
				m.GuildId == guildId && m.CharacterId == c.CharacterId)
			);
			var count = untaken.Count();
			var rng = new Random().Next(1, count + 1);
			return untaken.Skip(rng).FirstOrDefaultAsync();
		}
		public async Task<IReadOnlyList<Wish>> GetWishesAsync(ulong guildId, int characterId)
		{
			using var context = new GachaContext(_Options);

			var filtered = context.Wishes.Where(x => 
				x.User.GuildId == guildId && x.Character.CharacterId == characterId);
			return await filtered.ToArrayAsync().CAF();
		}
		public ValueTask<User> GetUserAsync(ulong guildId, ulong userId)
		{
			using var context = new GachaContext(_Options);

			return context.Users.FindAsync(guildId, userId);
		}
		public ValueTask<Marriage> GetMarriageAsync(ulong guildId, int characterId)
		{
			using var context = new GachaContext(_Options);

			return context.Marriages.FindAsync(guildId, characterId);
		}
		public Task<Source> GetSourceAsync(int sourceId)
		{
			using var context = new GachaContext(_Options);

			return context.Sources
				.Include(x => x.Characters)
					.ThenInclude(x => x.Images)
						.ThenInclude(x => x.Character)
				.SingleOrDefaultAsync(x => x.SourceId == sourceId);
		}
		public Task<Character> GetCharacterAsync(int characterId)
		{
			using var context = new GachaContext(_Options);

			return context.Characters
				.Include(x => x.Images)
					.ThenInclude(x => x.Character)
				.Include(x => x.Source)
				.SingleOrDefaultAsync(x => x.CharacterId == characterId);
		}

		public async Task<CharacterMetadata> GetCharacterMetadataAsync(Character character)
		{
			using var context = new GachaContext(_Options);

			var claims = context.Marriages.GetRankAsync(character.CharacterId, "Claims");
			var likes = new AmountAndRank("Likes", -1, -1);
			var wishes = context.Wishes.GetRankAsync(character.CharacterId, "Wishes");
			return new CharacterMetadata(character, claims, likes, wishes);
		}
	}

	public static class GachaDatabaseUtils
	{
		public static Task<Character> GetRandomCharacterAsync(
			this GachaDatabase db,
			IGuild guild)
			=> db.GetRandomCharacterAsync(guild.Id);
		public static Task<IReadOnlyList<Wish>> GetWishesAsync(
			this GachaDatabase db,
			IGuild guild,
			Character character)
			=> db.GetWishesAsync(guild.Id, character.CharacterId);
		public static ValueTask<User> GetUserAsync(
			this GachaDatabase db,
			IGuildUser user)
			=> db.GetUserAsync(user.GuildId, user.Id);
		public static ValueTask<Marriage> GetMarriageAsync(
			this GachaDatabase db,
			IGuild guild,
			Character character)
			=> db.GetMarriageAsync(guild.Id, character.CharacterId);
	}
}
