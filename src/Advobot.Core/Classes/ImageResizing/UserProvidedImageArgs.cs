using ImageMagick;

namespace Advobot.Classes.ImageResizing
{
	/// <summary>
	/// Arguments the user can provide for image resizing/creation.
	/// </summary>
	public class UserProvidedImageArgs
	{
		/// <summary>
		/// How many times to try resizing before giving up.
		/// </summary>
		public int ResizeTries { get; set; } = 3;
		/// <summary>
		/// How much before colors should be considered the same.
		/// </summary>
		public Percentage ColorFuzzing { get; set; } = new Percentage(30);
		/// <summary>
		/// When the gif should be started.
		/// </summary>
		public double StartInSeconds { get; set; } = 0;
		/// <summary>
		/// How long the gif should be.
		/// </summary>
		public double LengthInSeconds { get; set; } = 10;
	}
}