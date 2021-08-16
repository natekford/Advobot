using System.Threading.Tasks;

using Advobot.AutoMod.Attributes.ParameterPreconditions;
using Advobot.AutoMod.Database;
using Advobot.AutoMod.Models;
using Advobot.Punishments;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.AutoMod.ParameterPreconditions
{
	public abstract class NotAlreadyBannedPhraseAttribute_Tests<T>
		: ParameterPreconditionTestsBase<T>
		where T : NotAlreadyBannedPhraseParameterPreconditionAttribute
	{
		private readonly FakeAutoModDatabase _Db = new();

		protected abstract bool IsName { get; }
		protected abstract bool IsRegex { get; }
		protected abstract bool IsString { get; }

		[TestMethod]
		public async Task Existing_Test()
		{
			const string PHRASE = "hi";

			await _Db.UpsertBannedPhraseAsync(new BannedPhrase
			(
				GuildId: Context.Guild.Id,
				IsContains: IsName || IsString,
				IsName: IsName,
				IsRegex: IsRegex,
				Phrase: PHRASE,
				PunishmentType: PunishmentType.Nothing
			)).CAF();

			var result = await CheckPermissionsAsync(PHRASE).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task NotExisting_Test()
		{
			var result = await CheckPermissionsAsync("not existing").CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		[TestMethod]
		public async Task NotExistingButOtherTypeExists_Test()
		{
			const string PHRASE = "hi";

			await _Db.UpsertBannedPhraseAsync(new BannedPhrase
			(
				GuildId: Context.Guild.Id,
				IsContains: !(IsName || IsString),
				IsName: !IsName,
				IsRegex: !IsRegex,
				Phrase: PHRASE,
				PunishmentType: PunishmentType.Nothing
			)).CAF();

			var result = await CheckPermissionsAsync(PHRASE).CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		protected override void ModifyServices(IServiceCollection services)
		{
			services
				.AddSingleton<IAutoModDatabase>(_Db);
		}
	}
}