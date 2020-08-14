using System.Threading.Tasks;

using Advobot.AutoMod.Attributes.ParameterPreconditions;
using Advobot.AutoMod.Database;
using Advobot.AutoMod.Models;
using Advobot.Punishments;
using Advobot.Tests.Commands.AutoMod.Fakes;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.AutoMod.Attributes
{
	public abstract class NotAlreadyBannedPhraseAttribute_Tests<T>
		: ParameterlessParameterPreconditions_TestsBase<T>
		where T : NotAlreadyBannedPhraseParameterPreconditionAttribute, new()
	{
		private readonly FakeAutoModDatabase _Db = new FakeAutoModDatabase();

		protected abstract bool IsName { get; }
		protected abstract bool IsRegex { get; }
		protected abstract bool IsString { get; }

		protected NotAlreadyBannedPhraseAttribute_Tests()
		{
			Services = new ServiceCollection()
				.AddSingleton<IAutoModDatabase>(_Db)
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task Existing_Test()
		{
			const string PHRASE = "hi";

			await _Db.UpsertBannedPhraseAsync(new BannedPhrase
			{
				GuildId = Context.Guild.Id,
				Phrase = PHRASE,
				PunishmentType = PunishmentType.Nothing,
				IsRegex = IsRegex,
				IsName = IsName,
				IsContains = IsName || IsString,
			}).CAF();

			var result = await CheckAsync(PHRASE).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task FailsOnNotString_Test()
			=> await AssertPreconditionFailsOnInvalidType(CheckAsync(1)).CAF();

		[TestMethod]
		public async Task NotExisting_Test()
		{
			var result = await CheckAsync("not existing").CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		[TestMethod]
		public async Task NotExistingButOtherTypeExists_Test()
		{
			const string PHRASE = "hi";

			await _Db.UpsertBannedPhraseAsync(new BannedPhrase
			{
				GuildId = Context.Guild.Id,
				Phrase = PHRASE,
				PunishmentType = PunishmentType.Nothing,
				IsRegex = !IsRegex,
				IsName = !IsName,
				IsContains = !(IsName || IsString),
			}).CAF();

			var result = await CheckAsync(PHRASE).CAF();
			Assert.IsTrue(result.IsSuccess);
		}
	}
}