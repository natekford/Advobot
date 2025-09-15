using Advobot.AutoMod.Database;
using Advobot.AutoMod.Database.Models;
using Advobot.Modules;
using Advobot.TypeReaders.Discord;

using Microsoft.Extensions.DependencyInjection;

using MorseCode.ITask;

using YACCS.TypeReaders;

namespace Advobot.AutoMod.TypeReaders;

/// <summary>
/// A type reader for banned phrases.
/// </summary>
public abstract class BannedPhraseTypeReaderBase : DiscordTypeReader<BannedPhrase>
{
	/// <summary>
	/// Gets the name of the banned phrase type.
	/// </summary>
	protected abstract string BannedPhraseName { get; }

	/// <inheritdoc />
	public override async ITask<ITypeReaderResult<BannedPhrase>> ReadAsync(
		IGuildContext context,
		ReadOnlyMemory<string> input)
	{
		var joined = Join(context, input);

		var db = GetDatabase(context.Services);
		var phrases = await db.GetBannedPhrasesAsync(context.Guild.Id).ConfigureAwait(false);

		var matches = phrases
			.Where(x => IsValid(x, joined))
			.ToArray();
		return SingleValidResult(matches);
	}

	/// <summary>
	/// Determines if this phrase is valid.
	/// </summary>
	/// <param name="phrase"></param>
	/// <param name="input"></param>
	/// <returns></returns>
	protected abstract bool IsValid(BannedPhrase phrase, string input);

	[GetServiceMethod]
	private static AutoModDatabase GetDatabase(IServiceProvider services)
		=> services.GetRequiredService<AutoModDatabase>();
}