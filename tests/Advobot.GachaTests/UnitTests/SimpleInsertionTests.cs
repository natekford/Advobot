using Advobot.Gacha.Utils;
using AdvorangesUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.GachaTests.UnitTests
{
	[TestClass]
	public class SimpleInsertionTests : DatabaseTestsBase
	{
		[TestMethod]
		public async Task UserInsertionAndRetrieval_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var fakeUser = GenerateFakeUser();
			await db.AddUserAsync(fakeUser).CAF();

			var retrieved = await db.GetUserAsync(fakeUser.GetGuildId(), fakeUser.GetUserId()).CAF();
			Assert.AreNotEqual(null, retrieved);
			Assert.AreEqual(fakeUser.UserId, retrieved.UserId);
			Assert.AreEqual(fakeUser.GuildId, retrieved.GuildId);
		}
		[TestMethod]
		public async Task SourceInsertionAndRetrieval_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var fakeSource = GenerateFakeSource();
			fakeSource.SourceId = await db.AddSourceAsync(fakeSource).CAF();

			var retrieved = await db.GetSourceAsync(fakeSource.SourceId).CAF();
			Assert.AreNotEqual(null, retrieved);
			Assert.AreEqual(fakeSource.SourceId, retrieved.SourceId);
			Assert.AreEqual(fakeSource.Name, retrieved.Name);
			Assert.AreEqual(fakeSource.ThumbnailUrl, retrieved.ThumbnailUrl);
			Assert.AreEqual(fakeSource.TimeCreated, retrieved.TimeCreated);
		}
		[TestMethod]
		public async Task CharacterInsertionAndRetrieval_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var fakeSource = GenerateFakeSource();
			var fakeCharacter = GenerateFakeCharacter(fakeSource);
			fakeCharacter.CharacterId = await db.AddCharacterAsync(fakeCharacter).CAF();

			var retrieved = await db.GetCharacterAsync(fakeCharacter.CharacterId).CAF();
			Assert.AreNotEqual(null, retrieved);
			Assert.AreEqual(fakeCharacter.SourceId, retrieved.SourceId);
			Assert.AreEqual(fakeCharacter.CharacterId, retrieved.CharacterId);
			Assert.AreEqual(fakeCharacter.Name, retrieved.Name);
			Assert.AreEqual(fakeCharacter.GenderIcon, retrieved.GenderIcon);
			Assert.AreEqual(fakeCharacter.Gender, retrieved.Gender);
			Assert.AreEqual(fakeCharacter.RollType, retrieved.RollType);
			Assert.AreEqual(fakeCharacter.FlavorText, retrieved.FlavorText);
			Assert.AreEqual(fakeCharacter.IsFakeCharacter, retrieved.IsFakeCharacter);
			Assert.AreEqual(fakeCharacter.TimeCreated, retrieved.TimeCreated);
		}
		[TestMethod]
		public async Task ClaimInsertionAndRetrieval_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var fakeSource = GenerateFakeSource();
			var fakeCharacter = GenerateFakeCharacter(fakeSource);
			var fakeUser = GenerateFakeUser();
			var fakeClaim = GenerateFakeClaim(fakeUser, fakeCharacter);
			await db.AddClaimAsync(fakeClaim).CAF();

			var retrieved = await db.GetClaimAsync(fakeUser, fakeCharacter).CAF();
			Assert.AreNotEqual(null, retrieved);
			Assert.AreEqual(fakeClaim.GuildId, retrieved.GuildId);
			Assert.AreEqual(fakeClaim.UserId, retrieved.UserId);
			Assert.AreEqual(fakeClaim.CharacterId, retrieved.CharacterId);
			Assert.AreEqual(fakeClaim.ImageUrl, retrieved.ImageUrl);
			Assert.AreEqual(fakeClaim.IsPrimaryClaim, retrieved.IsPrimaryClaim);
			Assert.AreEqual(fakeClaim.TimeCreated, retrieved.TimeCreated);
		}
		[TestMethod]
		public async Task WishInsertionAndRetrieval_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var fakeSource = GenerateFakeSource();
			var fakeCharacter = GenerateFakeCharacter(fakeSource);
			var fakeUser = GenerateFakeUser();
			var fakeWish = GenerateFakeWish(fakeUser, fakeCharacter);
			await db.AddWishAsync(fakeWish).CAF();

			var retrievedList = await db.GetWishesAsync(fakeUser).CAF();
			var retrieved = retrievedList.SingleOrDefault(x => x.CharacterId == fakeCharacter.CharacterId);
			Assert.AreNotEqual(null, retrieved);
			Assert.AreEqual(fakeWish.GuildId, retrieved.GuildId);
			Assert.AreEqual(fakeWish.UserId, retrieved.UserId);
			Assert.AreEqual(fakeWish.CharacterId, retrieved.CharacterId);
			Assert.AreEqual(fakeWish.TimeCreated, retrieved.TimeCreated);
		}
		[TestMethod]
		public async Task ImageInsertionAndRetrieval_Test()
		{

		}
	}
}
