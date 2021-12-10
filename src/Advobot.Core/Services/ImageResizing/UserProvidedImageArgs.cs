using Discord.Commands;

using ImageMagick;

namespace Advobot.Services.ImageResizing;

/// <summary>
/// Arguments the user can provide for image resizing/creation.
/// </summary>
[NamedArgumentType]
public sealed class UserProvidedImageArgs
{
	/// <summary>
	/// How much before colors should be considered the same.
	/// </summary>
	public Percentage ColorFuzzing { get; set; } = new(30);

	/// <summary>
	/// How long the gif should be.
	/// </summary>
	public double LengthInSeconds { get; set; } = 10;

	/// <summary>
	/// How many times to try resizing before giving up.
	/// </summary>
	public int ResizeTries { get; set; } = 3;

	/// <summary>
	/// When the gif should be started.
	/// </summary>
	public double StartInSeconds { get; set; } = 0;
}