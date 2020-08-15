using System.Threading.Tasks;

using Advobot.AutoMod.Database;
using Advobot.AutoMod.Models;
using Advobot.AutoMod.TypeReaders;
using Advobot.Punishments;
using Advobot.Tests.Commands.AutoMod.Fakes;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders.BannedPhraseTypeReaders
{
	[TestClass]
	public abstract class BannedPhraseTypeReader_Tests : TypeReaderTestsBase
	{
		private readonly FakeAutoModDatabase _Db = new FakeAutoModDatabase();

		protected abstract bool IsName { get; }
		protected abstract bool IsRegex { get; }
		protected abstract bool IsString { get; }

		[TestMethod]
		public async Task Existing_Test()
		{
			const string PHRASE = "asdf";

			await _Db.UpsertBannedPhraseAsync(new BannedPhrase
			{
				GuildId = Context.Guild.Id,
				Phrase = PHRASE,
				PunishmentType = PunishmentType.Nothing,
				IsRegex = IsRegex,
				IsName = IsName,
				IsContains = IsName || IsString,
			}).CAF();

			var result = await ReadAsync(PHRASE).CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(BannedPhrase));
		}

		[TestMethod]
		public async Task NotExistingButOtherTypeExists_Test()
		{
			const string PHRASE = "asdf";

			await _Db.UpsertBannedPhraseAsync(new BannedPhrase
			{
				GuildId = Context.Guild.Id,
				Phrase = PHRASE,
				PunishmentType = PunishmentType.Nothing,
				IsRegex = !IsRegex,
				IsName = !IsName,
				IsContains = !(IsName || IsString),
			}).CAF();

			var result = await ReadAsync(PHRASE).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		protected override void ModifyServices(IServiceCollection services)
		{
			services
				.AddSingleton<IAutoModDatabase>(_Db);
		}
	}
}