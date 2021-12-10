namespace Advobot.Formatting;

/// <summary>
/// Holds a title and value.
/// </summary>
public sealed class Information
{
	/// <summary>
	/// The name of this information.
	/// </summary>
	public string Title { get; }

	/// <summary>
	/// The value of this information.
	/// </summary>
	public string Value { get; }

	/// <summary>
	/// Creates an instance of <see cref="Information"/>.
	/// </summary>
	/// <param name="title"></param>
	/// <param name="value"></param>
	public Information(string title, string value)
	{
		Title = title;
		Value = value;
	}

	/// <inheritdoc />
	public override string ToString()
		=> ToString(InformationMatrixFormattingArgs.Default);

	/// <inheritdoc />
	public string ToString(InformationMatrixFormattingArgs args)
		=> args.TitleFormatter(Title) + args.TitleAndValueSeparator + Value;
}