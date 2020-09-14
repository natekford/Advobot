using System;
using System.Linq;
using System.Threading.Tasks;

using Advobot.AutoMod.Database;
using Advobot.AutoMod.ReadOnlyModels;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

using static Advobot.Resources.Responses;

namespace Advobot.AutoMod.TypeReaders
{
	/// <summary>
	/// A type reader for banned names.
	/// </summary>
	public sealed class BannedNameTypeReader : BannedPhraseTypeReaderBase
	{
		/// <inheritdoc />
		protected override string BannedPhraseName => BannedPhraseVariableName;

		/// <inheritdoc />
		protected override bool IsValid(IReadOnlyBannedPhrase phrase, string input)
			=> phrase.IsName && phrase.Phrase == input;
	}

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
			var phrases = await db.GetBannedPhrasesAsync(context.Guild.Id).CAF();
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
		protected abstract bool IsValid(IReadOnlyBannedPhrase phrase, string input);
	}

	/// <summary>
	/// A type reader for banned regex.
	/// </summary>
	public sealed class BannedRegexTypeReader : BannedPhraseTypeReaderBase
	{
		/// <inheritdoc />
		protected override string BannedPhraseName => BannedPhraseVariableRegex;

		/// <inheritdoc />
		protected override bool IsValid(IReadOnlyBannedPhrase phrase, string input)
			=> !phrase.IsName && phrase.IsRegex && phrase.Phrase == input;
	}

	/// <summary>
	/// A type reader for banned strings.
	/// </summary>
	public sealed class BannedStringTypeReader : BannedPhraseTypeReaderBase
	{
		/// <inheritdoc />
		protected override string BannedPhraseName => BannedPhraseVariableString;

		/// <inheritdoc />
		protected override bool IsValid(IReadOnlyBannedPhrase phrase, string input)
			=> !phrase.IsName && !phrase.IsRegex && phrase.Phrase == input;
	}
}