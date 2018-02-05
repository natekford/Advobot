using Advobot.Core.Classes.Attributes;
using ImageMagick;
using System;
using System.Collections.Immutable;

namespace Advobot.Core.Classes
{
	public interface IImageResizerArgs
	{
		ImmutableArray<MagickFormat> ValidFormats { get; }
		long MaxAllowedLengthInBytes { get; }
		int ResizeTries { get; set; }
		Percentage ColorFuzzing { get; set; }
	}

	public sealed class IconResizerArgs : IImageResizerArgs
	{
		public ImmutableArray<MagickFormat> ValidFormats => ImmutableArray.Create(new[]
		{
			MagickFormat.Png,
			MagickFormat.Jpg,
			MagickFormat.Jpeg
		});
		public long MaxAllowedLengthInBytes => 10000000;

		public int ResizeTries { get; set; }
		public Percentage ColorFuzzing { get; set; }

		public IconResizerArgs() { }

		public static IconResizerArgs Default => new IconResizerArgs
		{
			ResizeTries = 5,
			ColorFuzzing = new Percentage(30),
		};
	}

	public sealed class EmoteResizerArgs : IImageResizerArgs
	{
		public ImmutableArray<MagickFormat> ValidFormats => ImmutableArray.Create(new[]
		{
			MagickFormat.Png,
			MagickFormat.Jpg,
			MagickFormat.Jpeg,
			MagickFormat.Mp4,
			MagickFormat.Gif,
		});
		public long MaxAllowedLengthInBytes => 256000;

		public int ResizeTries { get; set; }
		public Percentage ColorFuzzing { get; set; }
		public int AnimationDelay { get; set; }
		public int StartInSeconds { get; set; }
		public int LengthInSeconds { get; set; }

		public EmoteResizerArgs() { }
		[NamedArgumentConstructor]
		public EmoteResizerArgs(
			int resizeTries,
			Percentage colorFuzzing,
			[NamedArgument] uint? animationDelay,
			[NamedArgument] uint? startInSeconds,
			[NamedArgument] uint? lengthInSeconds)
		{
			ResizeTries = resizeTries;
			ColorFuzzing = colorFuzzing;
			AnimationDelay = (int)Math.Min(100, animationDelay ?? 8);
			StartInSeconds = (int)Math.Min(int.MaxValue, startInSeconds ?? 0);
			LengthInSeconds = (int)Math.Min(int.MaxValue, lengthInSeconds ?? 10);
		}

		public static EmoteResizerArgs Default => new EmoteResizerArgs
		{
			ResizeTries = 5,
			ColorFuzzing = new Percentage(30),
			AnimationDelay = 8,
			StartInSeconds = 0,
			LengthInSeconds = 10,
		};
	}
}
