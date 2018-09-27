using System.Collections.Immutable;
using Advobot.Interfaces;
using ImageMagick;

namespace Advobot.Classes.ImageResizing
{
	/// <summary>
	/// Arguments used when resizing an emote.
	/// </summary>
	public sealed class EmoteResizerArguments : IImageResizerArguments
	{
		/// <inheritdoc />
		public long MaxAllowedLengthInBytes => 256000;
		/// <inheritdoc />
		public ImmutableArray<MagickFormat> ValidFormats => ImmutableArray.Create(new[]
		{
			MagickFormat.Png,
			MagickFormat.Jpg,
			MagickFormat.Jpeg,
			MagickFormat.Mp4,
			MagickFormat.Gif,
		});
		/// <inheritdoc />
		public int ResizeTries { get; set; } = 5;
		/// <inheritdoc />
		public Percentage ColorFuzzing { get; set; } = new Percentage(30);
		/// <summary>
		/// When to start the emote.
		/// </summary>
		public int StartInSeconds { get; set; } = 0;
		/// <summary>
		/// How long to make the emote.
		/// </summary>
		public int LengthInSeconds { get; set; } = 10;
	}
}
