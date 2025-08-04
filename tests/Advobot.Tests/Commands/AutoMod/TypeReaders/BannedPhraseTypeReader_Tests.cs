using Advobot.AutoMod.Database;
using Advobot.AutoMod.Models;
using Advobot.AutoMod.TypeReaders;
using Advobot.Punishments;
using Advobot.Tests.Commands.AutoMod;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.Core.TypeReaders.BannedPhraseTypeReaders;

[TestClass]
public abstract class BannedPhraseTypeReader_Tests<T> : TypeReader_Tests<T>
	where T : BannedPhraseTypeReaderBase
{
	private readonly FakeAutoModDatabase _Db = new();

	protected abstract bool IsName { get; }
	protected abstract bool IsRegex { get; }
	protected abstract bool IsString { get; }

	[TestMethod]
	public async Task Existing_Test()
	{
		const string PHRASE = "asdf";

		await _Db.UpsertBannedPhraseAsync(new BannedPhrase
		(
			GuildId: Context.Guild.Id,
			IsContains: IsName || IsString,
			IsName: IsName,
			IsRegex: IsRegex,
			Phrase: PHRASE,
			PunishmentType: PunishmentType.Nothing
		)).CAF();

		var result = await ReadAsync(PHRASE).CAF();
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType(result.BestMatch, typeof(BannedPhrase));
	}

	[TestMethod]
	public async Task NotExistingButOtherTypeExists_Test()
	{
		const string PHRASE = "asdf";

		await _Db.UpsertBannedPhraseAsync(new BannedPhrase
		(
			GuildId: Context.Guild.Id,
			IsContains: !(IsName || IsString),
			IsName: !IsName,
			IsRegex: !IsRegex,
			Phrase: PHRASE,
			PunishmentType: PunishmentType.Nothing
		)).CAF();

		var result = await ReadAsync(PHRASE).CAF();
		Assert.IsFalse(result.IsSuccess);
	}

	protected override void ModifyServices(IServiceCollection services)
	{
		services
			.AddSingleton<IAutoModDatabase>(_Db);
	}
}