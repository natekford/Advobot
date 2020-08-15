using System.Linq;
using System.Threading.Tasks;

using Advobot.Gacha.Database;
using Advobot.Tests.Commands.Gacha.Utilities;
using Advobot.Tests.Fakes.Database;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Gacha.Database
{
	[TestClass]
	public sealed class CRUD_Tests
		: DatabaseTestsBase<GachaDatabase, FakeSQLiteConnectionString>
	{
		[TestMethod]
		public async Task CharacterCRUD_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var source = GachaTestUtils.GenerateFakeSource();
			var character = GachaTestUtils.GenerateFakeCharacter(source);
			await db.AddCharacterAsync(character).CAF();

			var retrieved = await db.GetCharacterAsync(character.CharacterId).CAF();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(character.SourceId, retrieved.SourceId);
			Assert.AreEqual(character.CharacterId, retrieved.CharacterId);
			Assert.AreEqual(character.Name, retrieved.Name);
			Assert.AreEqual(character.GenderIcon, retrieved.GenderIcon);
			Assert.AreEqual(character.Gender, retrieved.Gender);
			Assert.AreEqual(character.RollType, retrieved.RollType);
			Assert.AreEqual(character.FlavorText, retrieved.FlavorText);
			Assert.AreEqual(character.IsFakeCharacter, retrieved.IsFakeCharacter);
		}

		[TestMethod]
		public async Task ClaimCRUD_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var source = GachaTestUtils.GenerateFakeSource();
			var character = GachaTestUtils.GenerateFakeCharacter(source);
			var user = GachaTestUtils.GenerateFakeUser();
			var claim = GachaTestUtils.GenerateFakeClaim(user, character);
			await db.AddClaimAsync(claim).CAF();

			var retrieved = await db.GetClaimAsync(user, character).CAF();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(claim.ClaimId, retrieved.ClaimId);
			Assert.AreEqual(claim.GuildId, retrieved.GuildId);
			Assert.AreEqual(claim.UserId, retrieved.UserId);
			Assert.AreEqual(claim.CharacterId, retrieved.CharacterId);
			Assert.AreEqual(claim.ImageUrl, retrieved.ImageUrl);
			Assert.AreEqual(claim.IsPrimaryClaim, retrieved.IsPrimaryClaim);
		}

		[TestMethod]
		public async Task ImageCRUD_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var source = GachaTestUtils.GenerateFakeSource();
			var character = GachaTestUtils.GenerateFakeCharacter(source);
			var image = GachaTestUtils.GenerateFakeImage(character);
			await db.AddImageAsync(image).CAF();

			var retrievedList = await db.GetImagesAsync(character).CAF();
			var retrieved = retrievedList.Single();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(image.CharacterId, retrieved.CharacterId);
			Assert.AreEqual(image.Url, retrieved.Url);
		}

		[TestMethod]
		public async Task SourceCRUD_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var source = GachaTestUtils.GenerateFakeSource();
			await db.AddSourceAsync(source).CAF();

			var retrieved = await db.GetSourceAsync(source.SourceId).CAF();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(source.SourceId, retrieved.SourceId);
			Assert.AreEqual(source.Name, retrieved.Name);
			Assert.AreEqual(source.ThumbnailUrl, retrieved.ThumbnailUrl);
		}

		[TestMethod]
		public async Task UserCRUD_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var user = GachaTestUtils.GenerateFakeUser();
			await db.AddUserAsync(user).CAF();

			var retrieved = await db.GetUserAsync(user.GuildId, user.UserId).CAF();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(user.UserId, retrieved.UserId);
			Assert.AreEqual(user.GuildId, retrieved.GuildId);
		}

		[TestMethod]
		public async Task WishCRUD_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var source = GachaTestUtils.GenerateFakeSource();
			var character = GachaTestUtils.GenerateFakeCharacter(source);
			var user = GachaTestUtils.GenerateFakeUser();
			var wish = GachaTestUtils.GenerateFakeWish(user, character);
			await db.AddWishAsync(wish).CAF();

			var retrievedList = await db.GetWishesAsync(user).CAF();
			var retrieved = retrievedList.Single(x => x.CharacterId == character.CharacterId);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(wish.WishId, retrieved.WishId);
			Assert.AreEqual(wish.GuildId, retrieved.GuildId);
			Assert.AreEqual(wish.UserId, retrieved.UserId);
			Assert.AreEqual(wish.CharacterId, retrieved.CharacterId);
		}
	}
}