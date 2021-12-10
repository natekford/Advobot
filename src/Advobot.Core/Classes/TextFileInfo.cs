using AdvorangesUtils;

namespace Advobot.Classes;

/// <summary>
/// Holds information about what to name and put in a text file.
/// </summary>
public sealed class TextFileInfo
{
	private string _Name = "Temp";
	private string _Text = "";

	/// <summary>
	/// The name of the text file. This may have invalid characters for file names in it, but Discord will just remove those.
	/// </summary>
	public string Name
	{
		get => _Name;
		set
		{
			var sanitized = value.Replace(' ', '_').TrimEnd('_');
			_Name = $"{sanitized}_{FormattingUtils.ToSaving()}.txt";
		}
	}
	/// <summary>
	/// The text of the text file.
	/// </summary>
	public string Text
	{
		get => _Text;
		set => _Text = value.Trim();
	}
}