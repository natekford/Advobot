using Advobot.Utilities;

namespace Advobot.Info;

/// <summary>
/// Holds a 1d collection of <see cref="Information"/>.
/// </summary>
public sealed class InfoCollection
{
	/// <summary>
	/// A row of an <see cref="InfoMatrix"/>.
	/// </summary>
	public List<Info> Information { get; set; } = [];

	/// <summary>
	/// Adds a new <see cref="Advobot.Info.Info"/>.
	/// </summary>
	/// <param name="title"></param>
	/// <param name="value"></param>
	public void Add(string title, string value)
	{
		if (!string.IsNullOrWhiteSpace(value))
		{
			Information.Add(new(title, value));
		}
	}

	/// <summary>
	/// Adds a new <see cref="Advobot.Info.Info"/> with the value's string representation.
	/// </summary>
	/// <param name="title"></param>
	/// <param name="value"></param>
	public void Add(string title, object? value)
		=> Add(title, value?.ToString() ?? "");

	/// <inheritdoc />
	public override string ToString()
		=> ToString(InfoFormattingArgs.Default);

	/// <inheritdoc />
	public string ToString(InfoFormattingArgs args)
		=> Information.Select(x => x.ToString(args)).Join(args.InformationSeparator);
}