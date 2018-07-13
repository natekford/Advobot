using Advobot.Classes.Attributes;
using Advobot.Interfaces;
using ImageMagick;
using System;
using System.Collections.Immutable;

namespace Advobot.Classes
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
		public int ResizeTries { get; set; }
		/// <inheritdoc />
		public Percentage ColorFuzzing { get; set; }
		/// <summary>
		/// When to start the emote.
		/// </summary>
		public int StartInSeconds { get; set; }
		/// <summary>
		/// How long to make the emote.
		/// </summary>
		public int LengthInSeconds { get; set; }

		/// <summary>
		/// Creates an instance of emote resizer arguments.
		/// </summary>
		public EmoteResizerArguments()
		{
			ResizeTries = 5;
			ColorFuzzing = new Percentage(30);
			StartInSeconds = 0;
			LengthInSeconds = 10;
		}
		/// <summary>
		/// Creates the object via user input.
		/// </summary>
		/// <param name="resizeTries"></param>
		/// <param name="colorFuzzing"></param>
		/// <param name="startInSeconds"></param>
		/// <param name="lengthInSeconds"></param>
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
