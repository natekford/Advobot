using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Gacha.Database;
using Advobot.Gacha.Models;
using Advobot.Gacha.Utilities;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Gacha.Utilities
{
	public static class GachaTestUtils
	{
		public static readonly Random Rng = new();

		public static async Task<(List<Source>, List<Character>)> AddSourcesAndCharacters(
			this GachaDatabase db,
			int sourceCount,
			int charactersPerSource)
		{
			var sources = new List<Source>();
			var characters = new List<Character>();
			for (var i = 0; i < sourceCount; ++i)
			{
				var source = GenerateFakeSource();
				sources.Add(source);

				for (var j = 0; j < charactersPerSource; ++j)
				{
					characters.Add(GenerateFakeCharacter(source));
				}
			}
			var addedSources = await db.AddSourcesAsync(sources).CAF();
			Assert.AreEqual(sourceCount, addedSources);
			var addedCharacters = await db.AddCharactersAsync(characters).CAF();
			Assert.AreEqual(sourceCount * charactersPerSource, addedCharacters);
			return (sources, characters);
		}

		public static Character GenerateFakeCharacter(
			Source fakeSource,
			long? characterId = null)
		{
			return new()
			{
				SourceId = fakeSource.SourceId,
				CharacterId = characterId ?? TimeUtils.UtcNowTicks,
				Name = Guid.NewGuid().ToString(),
				GenderIcon = "\uD83D\uDE39",
				Gender = Gender.Other,
				RollType = RollType.All,
				IsFakeCharacter = true,
			};
		}

		public static Claim GenerateFakeClaim(
			User user,
			Character character)
		{
			return new(user, character)
			{
				IsPrimaryClaim = Rng.NextBool(),
			};
		}

		public static Image GenerateFakeImage(Character character)
		{
			var width = Rng.Next(1, 500);
			var height = Rng.Next(1, 500);

			return new(character)
			{
				Url = $"https://placekitten.com/{width}/{height}",
			};
		}

		public static Source GenerateFakeSource(long? sourceId = null)
		{
			return new()
			{
				SourceId = sourceId ?? TimeUtils.UtcNowTicks,
				Name = Guid.NewGuid().ToString(),
			};
		}

		public static User GenerateFakeUser(
			ulong? userId = null,
			ulong? guildId = null)
		{
			return new()
			{
				UserId = userId ?? Rng.NextUlongAboveLongMax(),
				GuildId = guildId ?? Rng.NextUlongAboveLongMax(),
			};
		}

		public static Wish GenerateFakeWish(User user, Character character)
			=> new(user, character);

		public static bool NextBool(this Random rng)
			=> rng.NextDouble() >= 0.5;

		/*
		public static ulong NextUlong(this Random rng)
		{
			var buffer = new byte[sizeof(ulong)];
			rng.NextBytes(buffer);
			return BitConverter.ToUInt64(buffer, 0);
		}*/

		public static ulong NextUlongAboveLongMax(this Random rng)
		{
			var buffer = new byte[sizeof(ulong)];
			rng.NextBytes(buffer);
			var value = BitConverter.ToUInt64(buffer, 0);
			if (value < long.MaxValue)
			{
				value += long.MaxValue;
			}

			return value;
		}
	}
}