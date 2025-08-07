namespace Advobot.Formatting;

/// <summary>
/// Holds a title and value.
/// </summary>
/// <remarks>
/// Creates an instance of <see cref="Info"/>.
/// </remarks>
/// <param name="Title">The name of this information.</param>
/// <param name="Value">The value of this information.</param>
public sealed record Info(string Title, string Value)
{
	/// <inheritdoc />
	public override string ToString()
		=> ToString(InfoFormattingArgs.Default);

	/// <inheritdoc />
	public string ToString(InfoFormattingArgs args)
		=> args.TitleFormatter(Title) + args.TitleAndValueSeparator + Value;
}