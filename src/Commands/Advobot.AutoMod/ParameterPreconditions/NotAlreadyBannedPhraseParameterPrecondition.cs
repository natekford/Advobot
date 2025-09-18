using Advobot.AutoMod.Database;
using Advobot.AutoMod.Database.Models;
using Advobot.Modules;
using Advobot.ParameterPreconditions;
using Advobot.Utilities;

using Microsoft.Extensions.DependencyInjection;

using YACCS.Preconditions;
using YACCS.Results;
using YACCS.TypeReaders;

using static Advobot.Resources.Responses;

namespace Advobot.AutoMod.ParameterPreconditions;

/// <summary>
/// Makes sure the passed in <see cref="string"/> is not already a banned phrase.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public abstract class NotAlreadyBannedPhraseParameterPrecondition
	: AdvobotParameterPrecondition<string>
{
	/// <inheritdoc />
	public override string Summary => BannedPhraseNotExisting.Format(BannedPhraseName.WithNoMarkdown());
	/// <summary>
	/// Gets the name of the banned phrase type.
	/// </summary>
	protected abstract string BannedPhraseName { get; }

	/// <inheritdoc />
	protected override async ValueTask<IResult> CheckNotNullAsync(
		CommandMeta meta,
		IGuildContext context,
		string value)
	{
		var db = GetDatabase(context.Services);
		var phrases = await db.GetBannedPhrasesAsync(context.Guild.Id).ConfigureAwait(false);
		if (phrases.Any(x => IsMatch(x, value)))
		{
			return Result.Failure(BannedPhraseAlreadyExists.Format(
				value.WithBlock(),
				BannedPhraseName.WithNoMarkdown()
			));
		}
		return Result.EmptySuccess;
	}

	/// <summary>
	/// Gets the phrases this should look through.
	/// </summary>
	/// <param name="phrase"></param>
	/// <param name="input"></param>
	/// <returns></returns>
	protected abstract bool IsMatch(BannedPhrase phrase, string input);

	[GetServiceMethod]
	private static AutoModDatabase GetDatabase(IServiceProvider services)
		=> services.GetRequiredService<AutoModDatabase>();
}