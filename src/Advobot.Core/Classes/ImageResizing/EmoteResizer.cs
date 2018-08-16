using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdvorangesUtils;
using Discord;
using ImageMagick;

namespace Advobot.Classes.ImageResizing
{
	/// <summary>
	/// Used for resizing emotes to upload to a server.
	/// </summary>
	public sealed class EmoteResizer : ImageResizer<EmoteResizerArguments>
	{
		/// <summary>
		/// Creates an instance of <see cref="EmoteResizer"/>.
		/// </summary>
		/// <param name="threads"></param>
		public EmoteResizer(int threads) : base(threads, "emote") { }

		/// <inheritdoc />
		protected override async Task<Error> UseResizedImageStream(AdvobotCommandContext context, MemoryStream stream, MagickFormat format, string name, RequestOptions options)
		{
			switch (format)
			{
				case MagickFormat.Jpg:
				case MagickFormat.Jpeg:
				case MagickFormat.Png:
					if (context.Guild.Emotes.Where(x => !x.Animated).Count() >= 50)
					{
						return new Error("there are already 50 non animated emotes.");
					}
					break;
				case MagickFormat.Gif:
					if (context.Guild.Emotes.Where(x => x.Animated).Count() >= 50)
					{
						return new Error("there are already 50 animated emotes.");
					}
					break;
			}
			await context.Guild.CreateEmoteAsync(name, new Image(stream), default, options).CAF();
			return null;
		}
	}
}