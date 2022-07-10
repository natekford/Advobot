using Advobot.Classes;

using AdvorangesUtils;

using Discord;

using System;
using System.Collections.Generic;
using System.Text;

namespace Advobot.EmbedWrapper;

internal static class EmbedRules
{
	public static EmbedPropertyValidator<TEmbed, string?> InvalidUrl<TEmbed>(
		this EmbedPropertyValidator<TEmbed, string?> validator)
	{
		return validator.Rule(
			x => x?.IsValidUrl() == false,
			() => "Invalid url."
		);
	}

	public static EmbedPropertyValidator<TEmbed, string?> Max<TEmbed>(
		this EmbedPropertyValidator<TEmbed, string?> validator,
		int max)
	{
		return validator.Rule(
			x => x?.Length > max,
			() => $"Max length is {max}."
		);
	}

	public static EmbedPropertyValidator<TEmbed, string?> MaxLines<TEmbed>(
		this EmbedPropertyValidator<TEmbed, string?> validator,
		int max)
	{
		return validator.Rule(
			x => x?.CountLineBreaks() > max,
			() => $"Max new lines is {max}."
		);
	}

	public static EmbedPropertyValidator<TEmbed, string?> Remaining<TEmbed>(
		this EmbedPropertyValidator<TEmbed, string?> validator,
		int remaining)
	{
		return validator.Rule(
			x => x?.Length > remaining,
			() => $"Remaining length is {remaining}."
		);
	}
}