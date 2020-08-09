using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Gacha.Database;
using Advobot.Gacha.Models;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Utilities;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Gacha.Utilities
{
	public static class GachaTestUtils
	{
		public static readonly Random Rng = new Random();

		public static async Task<(List<IReadOnlySource>, List<IReadOnlyCharacter>)> AddSourcesAndCharacters(
			this GachaDatabase db,
			int sourceCount,
			int charactersPerSource)
		{
			var sources = new List<IReadOnlySource>();
			var characters = new List<IReadOnlyCharacter>();
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

		public static IReadOnlyCharacter GenerateFakeCharacter(
					IReadOnlySource fakeSource,
			long? characterId = null)
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

		public static IReadOnlyClaim GenerateFakeClaim(
			IReadOnlyUser user,
			IReadOnlyCharacter character)
		{
			return new Claim(user, character)
			{
				IsPrimaryClaim = Rng.NextBool(),
			};
		}

		public static IReadOnlyImage GenerateFakeImage(IReadOnlyCharacter character)
		{
			var width = Rng.Next(1, 500);
			var height = Rng.Next(1, 500);

			return new Image(character)
			{
				Url = $"https://placekitten.com/{width}/{height}",
			};
		}

		public static IReadOnlySource GenerateFakeSource(long? sourceId = null)
		{
			return new Source
			{
				SourceId = sourceId ?? TimeUtils.UtcNowTicks,
				Name = Guid.NewGuid().ToString(),
			};
		}

		public static IReadOnlyUser GenerateFakeUser(
			ulong? userId = null,
			ulong? guildId = null)
		{
			return new User
			{
				UserId = (userId ?? Rng.NextUlongAboveLongMax()).ToString(),
				GuildId = (guildId ?? Rng.NextUlongAboveLongMax()).ToString(),
			};
		}

		public static IReadOnlyWish GenerateFakeWish(
			IReadOnlyUser user,
			IReadOnlyCharacter character)
			=> new Wish(user, character);

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