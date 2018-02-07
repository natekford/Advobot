using Advobot.Core.Interfaces;
using ImageMagick;
using System.Collections.Immutable;

namespace Advobot.Core.Classes
{
	public sealed class IconResizerArgs : IImageResizerArgs
	{
		public long MaxAllowedLengthInBytes => 10000000;
		public ImmutableArray<MagickFormat> ValidFormats => ImmutableArray.Create(new[]
		{
			MagickFormat.Png,
			MagickFormat.Jpg,
			MagickFormat.Jpeg
		});
		public int ResizeTries { get; set; }
		public Percentage ColorFuzzing { get; set; }

		public IconResizerArgs()
		{
			ResizeTries = 5;
			ColorFuzzing = new Percentage(30);
		}
	}
}
