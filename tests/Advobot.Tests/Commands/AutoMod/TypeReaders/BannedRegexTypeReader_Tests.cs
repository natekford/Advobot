using System.Threading.Tasks;

using Advobot.AutoMod.Database;
using Advobot.AutoMod.Models;
using Advobot.AutoMod.TypeReaders;
using Advobot.Punishments;
using Advobot.Tests.Commands.AutoMod.Fakes;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders.BannedPhraseTypeReaders
{
	[TestClass]
	public abstract class BannedPhraseTypeReader_Tests<T>
		: TypeReader_TestsBase<T>
		where T : BannedPhraseTypeReaderBase, new()
	{
		private readonly FakeAutoModDatabase _Db = new FakeAutoModDatabase();

		protected abstract bool IsName { get; }
		protected abstract bool IsRegex { get; }
		protected abstract bool IsString { get; }

		protected BannedPhraseTypeReader_Tests()
		{
			Services = new ServiceCollection()
				.AddSingleton<IAutoModDatabase>(_Db)
				.BuildServiceProvider();
		}

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
		public async Task NotExisting_Test()
		{
			var result = await ReadAsync("asdf").CAF();
			Assert.IsFalse(result.IsSuccess);
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
	}
}