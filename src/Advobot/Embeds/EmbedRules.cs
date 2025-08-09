namespace Advobot.Embeds;

internal static class EmbedRules
{
	public static EmbedPropertyValidator<TEmbed, string?> Max<TEmbed>(
		this EmbedPropertyValidator<TEmbed, string?> validator,
		int max)
	{
		return validator.Rule(
			x => x?.Length <= max,
			() => $"Max length is {max}."
		);
	}

	public static EmbedPropertyValidator<TEmbed, int> Max<TEmbed>(
		this EmbedPropertyValidator<TEmbed, int> validator,
		int max)
	{
		return validator.Rule(
			x => x <= max,
			() => $"Max count is {max}."
		);
	}

	public static EmbedPropertyValidator<TEmbed, string?> MaxLines<TEmbed>(
		this EmbedPropertyValidator<TEmbed, string?> validator,
		int max)
	{
		return validator.Rule(
			x => x?.Count(c => c is '\r' or '\n') <= max,
			() => $"Max new lines is {max}."
		);
	}

	public static EmbedPropertyValidator<TEmbed, string?> NotEmpty<TEmbed>(
		this EmbedPropertyValidator<TEmbed, string?> validator)
	{
		return validator.Rule(
			x => !string.IsNullOrWhiteSpace(x),
			() => "Cannot be null or empty."
		);
	}

	public static EmbedPropertyValidator<TEmbed, string?> Remaining<TEmbed>(
		this EmbedPropertyValidator<TEmbed, string?> validator,
		int remaining)
	{
		return validator.Rule(
			x => x?.Length <= remaining,
			() => $"Remaining length is {remaining}."
		);
	}

	public static EmbedPropertyValidator<TEmbed, string?> ValidUrl<TEmbed>(
		this EmbedPropertyValidator<TEmbed, string?> validator)
	{
		return validator.Rule(
			x => x is null || (Uri.TryCreate(x, UriKind.Absolute, out var uri)
				&& (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)),
			() => "Invalid url."
		);
	}
}