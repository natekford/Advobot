using Advobot.AutoMod.Database;
using Advobot.AutoMod.Models;
using Advobot.Utilities;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

using static Advobot.Resources.Responses;

namespace Advobot.AutoMod.TypeReaders;

/// <summary>
/// A type reader for banned phrases.
/// </summary>
public abstract class BannedPhraseTypeReaderBase : TypeReader
{
	/// <summary>
	/// Gets the name of the banned phrase type.
	/// </summary>
	protected abstract string BannedPhraseName { get; }

	/// <inheritdoc />
	public override async Task<TypeReaderResult> ReadAsync(
		ICommandContext context,
		string input,
		IServiceProvider services)
	{
		var db = services.GetRequiredService<IAutoModDatabase>();
		var phrases = await db.GetBannedPhrasesAsync(context.Guild.Id).ConfigureAwait(false);
		var matches = phrases.Where(x => IsValid(x, input)).ToArray();

		var type = BannedPhraseType.Format(BannedPhraseName.WithNoMarkdown());
		return TypeReaderUtils.SingleValidResult(matches, type, input);
	}

	/// <summary>
	/// Determines if this phrase is valid.
	/// </summary>
	/// <param name="phrase"></param>
	/// <param name="input"></param>
	/// <returns></returns>
	protected abstract bool IsValid(BannedPhrase phrase, string input);
}