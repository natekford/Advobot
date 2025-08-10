using Advobot.Utilities;

namespace Advobot.Info;

/// <summary>
/// Arguments for formatting an information matrix.
/// </summary>
public sealed class InfoFormattingArgs
{
	/// <summary>
	/// The default args.
	/// </summary>
	public static readonly InfoFormattingArgs Default = new();
	/// <summary>
	/// The separator for entire collections.
	/// </summary>
	public string CollectionSeparator { get; set; } = "\n\n";
	/// <summary>
	/// The separator for information within collections.
	/// </summary>
	public string InformationSeparator { get; set; } = "\n";
	/// <summary>
	/// The separator for the title and value in information.
	/// </summary>
	public string TitleAndValueSeparator { get; set; } = " ";
	/// <summary>
	/// How to format the title of information.
	/// </summary>
	public Func<string, string> TitleFormatter { get; set; } = x => x.WithTitleCaseAndColon().ToString();
}