using Advobot.AutoMod.Database;
using Advobot.AutoMod.Models;
using Advobot.AutoMod.ParameterPreconditions;
using Advobot.Punishments;
using Advobot.Tests.TestBases;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.Commands.AutoMod.ParameterPreconditions;

public abstract class NotAlreadyBannedPhrase_Tests<T> : ParameterPrecondition_Tests<T>
	where T : NotAlreadyBannedPhraseParameterPrecondition
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
		)).ConfigureAwait(false);

		await AssertFailureAsync(PHRASE).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task NotExisting_Test()
		=> await AssertSuccessAsync("not existing").ConfigureAwait(false);

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
		)).ConfigureAwait(false);

		await AssertSuccessAsync(PHRASE).ConfigureAwait(false);
	}

	protected override void ModifyServices(IServiceCollection services)
	{
		services
			.AddSingleton<IAutoModDatabase>(_Db);
	}
}