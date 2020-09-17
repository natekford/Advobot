using System;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions;
using Advobot.AutoMod.Database;
using Advobot.AutoMod.ReadOnlyModels;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

using static Advobot.Resources.Responses;

namespace Advobot.AutoMod.Attributes.ParameterPreconditions
{
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
		protected override bool IsMatch(IReadOnlyBannedPhrase phrase, string input)
			=> phrase.IsName && phrase.Phrase == input;
	}

	/// <summary>
	/// Makes sure the passed in <see cref="string"/> is not already a banned phrase.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public abstract class NotAlreadyBannedPhraseParameterPreconditionAttribute
		: AdvobotParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string Summary => BannedPhraseNotExisting.Format(BannedPhraseName.WithNoMarkdown());
		/// <summary>
		/// Gets the name of the banned phrase type.
		/// </summary>
		protected abstract string BannedPhraseName { get; }

		/// <summary>
		/// Gets the phrases this should look through.
		/// </summary>
		/// <param name="phrase"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		protected abstract bool IsMatch(IReadOnlyBannedPhrase phrase, string input);

		/// <inheritdoc />
		protected override async Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			object value,
			IServiceProvider services)
		{
			if (!(value is string input))
			{
				return this.FromOnlySupports(value, typeof(string));
			}

			var db = services.GetRequiredService<IAutoModDatabase>();
			var phrases = await db.GetBannedPhrasesAsync(context.Guild.Id).CAF();
			if (phrases.Any(x => IsMatch(x, input)))
			{
				return PreconditionResult.FromError(BannedPhraseAlreadyExists.Format(
					input.WithBlock(),
					BannedPhraseName.WithNoMarkdown()
				));
			}
			return this.FromSuccess();
		}
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
		protected override bool IsMatch(IReadOnlyBannedPhrase phrase, string input)
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
		protected override bool IsMatch(IReadOnlyBannedPhrase phrase, string input)
			=> !phrase.IsRegex && phrase.Phrase == input;
	}
}