using Advobot.AutoMod.Database;
using Advobot.AutoMod.Models;
using Advobot.GeneratedParameterPreconditions;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

using static Advobot.Resources.Responses;

namespace Advobot.AutoMod.Attributes.ParameterPreconditions;

/// <summary>
/// Makes sure the passed in <see cref="string"/> is not already a banned name.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class NotAlreadyBannedNameAttribute
	: NotAlreadyBannedPhraseParameterPreconditionAttribute
{
	/// <inheritdoc />
	protected override string BannedPhraseName => VariableName;

	/// <inheritdoc />
	protected override bool IsMatch(BannedPhrase phrase, string input)
		=> phrase.IsName && phrase.Phrase == input;
}

/// <summary>
/// Makes sure the passed in <see cref="string"/> is not already a banned phrase.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public abstract class NotAlreadyBannedPhraseParameterPreconditionAttribute
	: StringParameterPreconditionAttribute
{
	/// <inheritdoc />
	public override string Summary => BannedPhraseNotExisting.Format(BannedPhraseName.WithNoMarkdown());
	/// <summary>
	/// Gets the name of the banned phrase type.
	/// </summary>
	protected abstract string BannedPhraseName { get; }

	/// <inheritdoc />
	protected override async Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		ParameterInfo parameter,
		IGuildUser invoker,
		string value,
		IServiceProvider services)
	{
		var db = services.GetRequiredService<IAutoModDatabase>();
		var phrases = await db.GetBannedPhrasesAsync(context.Guild.Id).CAF();
		if (phrases.Any(x => IsMatch(x, value)))
		{
			return PreconditionResult.FromError(BannedPhraseAlreadyExists.Format(
				value.WithBlock(),
				BannedPhraseName.WithNoMarkdown()
			));
		}
		return this.FromSuccess();
	}

	/// <summary>
	/// Gets the phrases this should look through.
	/// </summary>
	/// <param name="phrase"></param>
	/// <param name="input"></param>
	/// <returns></returns>
	protected abstract bool IsMatch(BannedPhrase phrase, string input);
}

/// <summary>
/// Makes sure the passed in <see cref="string"/> is not already a banned regex.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class NotAlreadyBannedRegexAttribute
	: NotAlreadyBannedPhraseParameterPreconditionAttribute
{
	/// <inheritdoc />
	protected override string BannedPhraseName => VariableRegex;

	/// <inheritdoc />
	protected override bool IsMatch(BannedPhrase phrase, string input)
		=> phrase.IsRegex && phrase.Phrase == input;
}

/// <summary>
/// Makes sure the passed in <see cref="string"/> is not already a banned string.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class NotAlreadyBannedStringAttribute
	: NotAlreadyBannedPhraseParameterPreconditionAttribute
{
	/// <inheritdoc />
	protected override string BannedPhraseName => VariableString;

	/// <inheritdoc />
	protected override bool IsMatch(BannedPhrase phrase, string input)
		=> !phrase.IsRegex && phrase.Phrase == input;
}