using AdvorangesUtils;

namespace Advobot.Formatting;

/// <summary>
/// Holds a 1d collection of <see cref="Information"/>.
/// </summary>
public sealed class InformationCollection
{
	private readonly List<Information> _Information = new();

	/// <summary>
	/// A row of an <see cref="InformationMatrix"/>.
	/// </summary>
	public IReadOnlyList<Information> Information => _Information.AsReadOnly();

	/// <summary>
	/// Adds a new <see cref="Formatting.Information"/>.
	/// </summary>
	/// <param name="title"></param>
	/// <param name="value"></param>
	public void Add(string title, string value)
	{
		if (!string.IsNullOrWhiteSpace(value))
		{
			_Information.Add(new Information(title, value));
		}
	}

	/// <summary>
	/// Adds a new <see cref="Formatting.Information"/> with the number's string value.
	/// </summary>
	/// <param name="title"></param>
	/// <param name="value"></param>
	public void Add(string title, int value)
		=> Add(title, value.ToString());

	/// <summary>
	/// Adds a new <see cref="Formatting.Information"/> with the bool's string value.
	/// </summary>
	/// <param name="title"></param>
	/// <param name="value"></param>
	public void Add(string title, bool value)
		=> Add(title, value.ToString());

	/// <inheritdoc />
	public override string ToString()
		=> ToString(InformationMatrixFormattingArgs.Default);

	/// <inheritdoc />
	public string ToString(InformationMatrixFormattingArgs args)
		=> _Information.Join(x => x.ToString(args), args.InformationSeparator);
}