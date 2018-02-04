using ImageMagick;

namespace Advobot.Core.Classes
{
	public sealed class ImageResizerArgs
	{
		/// <summary>
		/// The length in bits that the image can have before it's invalid.
		/// </summary>
		public long MaxSize;
		/// <summary>
		/// How many times to attempt and resize the image.
		/// </summary>
		public int ResizeTries;
		/// <summary>
		/// The delay in 1/100th of a second between frames.
		/// </summary>
		public int AnimationDelay = 10;
		/// <summary>
		/// The color fuzzing percentage to use when optimizing a gif.
		/// </summary>
		public Percentage ColorFuzzingPercentage;
		/// <summary>
		/// Skip every xth frame.
		/// </summary>
		public int FrameSkip;
	}
}
