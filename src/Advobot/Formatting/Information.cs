namespace Advobot.Formatting;

/// <summary>
/// Holds a title and value.
/// </summary>
/// <remarks>
/// Creates an instance of <see cref="Information"/>.
/// </remarks>
/// <param name="title"></param>
/// <param name="value"></param>
public sealed class Information(string title, string value)
{
	/// <summary>
	/// The name of this information.
	/// </summary>
	public string Title { get; } = title;

	/// <summary>
	/// The value of this information.
	/// </summary>
	public string Value { get; } = value;

	/// <inheritdoc />
	public override string ToString()
		=> ToString(InformationMatrixFormattingArgs.Default);

	/// <inheritdoc />
	public string ToString(InformationMatrixFormattingArgs args)
		=> args.TitleFormatter(Title) + args.TitleAndValueSeparator + Value;
}