using Advobot.AutoMod.Database;
using Advobot.AutoMod.Database.Models;
using Advobot.AutoMod.TypeReaders;
using Advobot.Punishments;
using Advobot.Tests.TestBases;
using Advobot.Tests.Utilities;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.Commands.AutoMod.TypeReaders;

[TestClass]
public abstract class BannedPhraseTypeReader_Tests<T> : TypeReader_Tests<T>
	where T : BannedPhraseTypeReaderBase
{
	protected abstract bool IsName { get; }
	protected abstract bool IsRegex { get; }
	protected abstract bool IsString { get; }

	[TestMethod]
	public async Task Existing_Test()
	{
		const string PHRASE = "asdf";

		var db = await GetDatabaseAsync().ConfigureAwait(false);
		await db.UpsertBannedPhraseAsync(new BannedPhrase
		(
			GuildId: Context.Guild.Id,
			IsContains: IsName || IsString,
			IsName: IsName,
			IsRegex: IsRegex,
			Phrase: PHRASE,
			PunishmentType: PunishmentType.Nothing
		)).ConfigureAwait(false);

		var result = await ReadAsync(PHRASE).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType(result.BestMatch, typeof(BannedPhrase));
	}

	[TestMethod]
	public async Task NotExistingButOtherTypeExists_Test()
	{
		const string PHRASE = "asdf";

		var db = await GetDatabaseAsync().ConfigureAwait(false);
		await db.UpsertBannedPhraseAsync(new BannedPhrase
		(
			GuildId: Context.Guild.Id,
			IsContains: !(IsName || IsString),
			IsName: !IsName,
			IsRegex: !IsRegex,
			Phrase: PHRASE,
			PunishmentType: PunishmentType.Nothing
		)).ConfigureAwait(false);

		var result = await ReadAsync(PHRASE).ConfigureAwait(false);
		Assert.IsFalse(result.IsSuccess);
	}

	protected Task<AutoModDatabase> GetDatabaseAsync()
		=> Services.GetDatabaseAsync<AutoModDatabase>();

	protected override Task SetupAsync()
		=> GetDatabaseAsync();
}