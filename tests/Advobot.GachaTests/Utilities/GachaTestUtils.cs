using System;
using Advobot.Gacha.Models;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Utilities;

namespace Advobot.GachaTests.Utilities
{
	public static class GachaTestUtils
	{
		public static readonly Random Rng = new Random();

		public static ulong NextUlong(this Random rng)
		{
			var buffer = new byte[sizeof(ulong)];
			rng.NextBytes(buffer);
			return BitConverter.ToUInt64(buffer, 0);
		}
		public static bool NextBool(this Random rng)
			=> rng.NextDouble() >= 0.5;
		public static IReadOnlySource GenerateFakeSource(long? sourceId = null)
		{
			return new Source
			{
				SourceId = sourceId ?? TimeUtils.UtcNowTicks,
				Name = Guid.NewGuid().ToString(),
			};
		}
		public static IReadOnlyCharacter GenerateFakeCharacter(IReadOnlySource fakeSource, long? characterId = null)
		{
			return new Character(fakeSource)
			{
				CharacterId = characterId ?? TimeUtils.UtcNowTicks,
				Name = Guid.NewGuid().ToString(),
				GenderIcon = "\uD83D\uDE39",
				Gender = Gender.Other,
				RollType = RollType.All,
				IsFakeCharacter = true,
			};
		}
		public static IReadOnlyUser GenerateFakeUser(ulong? userId = null, ulong? guildId = null)
		{
			return new User
			{
				UserId = (userId ?? Rng.NextUlong()).ToString(),
				GuildId = (guildId ?? Rng.NextUlong()).ToString(),
			};
		}
		public static IReadOnlyClaim GenerateFakeClaim(IReadOnlyUser user, IReadOnlyCharacter character)
		{
			return new Claim(user, character)
			{
				IsPrimaryClaim = Rng.NextBool(),
			};
		}
		public static IReadOnlyWish GenerateFakeWish(IReadOnlyUser user, IReadOnlyCharacter character)
			=> new Wish(user, character);
		public static IReadOnlyImage GenerateFakeImage(IReadOnlyCharacter character)
		{
			var width = Rng.Next(1, 500);
			var height = Rng.Next(1, 500);

			return new Image(character)
			{
				Url = $"https://placekitten.com/{width}/{height}",
			};
		}
	}
}
