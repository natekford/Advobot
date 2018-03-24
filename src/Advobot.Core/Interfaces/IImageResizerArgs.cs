using ImageMagick;
using System.Collections.Immutable;

namespace Advobot.Core.Interfaces
{
	/// <summary>
	/// Arguments to resize an image with.
	/// </summary>
	public interface IImageResizerArguments
	{
		/// <summary>
		/// Valid formats so an invalid image isn't attempted to resize.
		/// </summary>
		ImmutableArray<MagickFormat> ValidFormats { get; }
		/// <summary>
		/// The largest allowed file size.
		/// </summary>
		long MaxAllowedLengthInBytes { get; }
		/// <summary>
		/// How many times to try resizing before giving up.
		/// </summary>
		int ResizeTries { get; set; }
		/// <summary>
		/// How much before colors should be considered the same.
		/// </summary>
		Percentage ColorFuzzing { get; set; }
	}
}
