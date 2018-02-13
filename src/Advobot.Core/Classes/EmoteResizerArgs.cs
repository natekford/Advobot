using Advobot.Core.Classes.Attributes;
using Advobot.Core.Interfaces;
using ImageMagick;
using System;
using System.Collections.Immutable;

namespace Advobot.Core.Classes
{
	public sealed class EmoteResizerArguments : IImageResizerArguments
	{
		public long MaxAllowedLengthInBytes => 256000;
		public ImmutableArray<MagickFormat> ValidFormats => ImmutableArray.Create(new[]
		{
			MagickFormat.Png,
			MagickFormat.Jpg,
			MagickFormat.Jpeg,
			MagickFormat.Mp4,
			MagickFormat.Gif,
		});
		public int ResizeTries { get; set; }
		public Percentage ColorFuzzing { get; set; }
		public int StartInSeconds { get; set; }
		public int LengthInSeconds { get; set; }

		public EmoteResizerArguments()
		{
			ResizeTries = 5;
			ColorFuzzing = new Percentage(30);
			StartInSeconds = 0;
			LengthInSeconds = 10;
		}
		[NamedArgumentConstructor]
		public EmoteResizerArguments(
			int resizeTries,
			Percentage colorFuzzing,
			[NamedArgument] uint? startInSeconds,
			[NamedArgument] uint? lengthInSeconds)
		{
			ResizeTries = resizeTries;
			ColorFuzzing = colorFuzzing;
			StartInSeconds = (int)Math.Min(int.MaxValue, startInSeconds ?? 0);
			LengthInSeconds = (int)Math.Min(int.MaxValue, lengthInSeconds ?? 10);
		}
	}
}
