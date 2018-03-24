using Advobot.Core.Interfaces;
using ImageMagick;
using System.Collections.Immutable;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Arguments used when resizing an icon. For bots, guilds, etc.
	/// </summary>
	public sealed class IconResizerArguments : IImageResizerArguments
	{
		/// <inheritdoc />
		public long MaxAllowedLengthInBytes => 10000000;
		/// <inheritdoc />
		public ImmutableArray<MagickFormat> ValidFormats => ImmutableArray.Create(new[]
		{
			MagickFormat.Png,
			MagickFormat.Jpg,
			MagickFormat.Jpeg
		});
		/// <inheritdoc />
		public int ResizeTries { get; set; }
		/// <inheritdoc />
		public Percentage ColorFuzzing { get; set; }

		/// <summary>
		/// Creates an instance of icon resizer arguments.
		/// </summary>
		public IconResizerArguments()
		{
			ResizeTries = 5;
			ColorFuzzing = new Percentage(30);
		}
	}
}
