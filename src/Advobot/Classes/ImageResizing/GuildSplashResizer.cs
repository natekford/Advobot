using System.IO;
using System.Threading.Tasks;
using AdvorangesUtils;
using Discord;
using ImageMagick;

namespace Advobot.Classes.ImageResizing
{
	/// <summary>
	/// Used for resizing a guild splash and changing it.
	/// </summary>
	public sealed class GuildSplashResizer : ImageResizer<IconResizerArguments>
	{
		/// <summary>
		/// Creates an instance of <see cref="GuildSplashResizer"/>.
		/// </summary>
		/// <param name="threads"></param>
		public GuildSplashResizer(int threads) : base(threads, "guild splash") { }

		/// <inheritdoc />
		protected override async Task<Error> UseResizedImageStream(AdvobotSocketCommandContext context, MemoryStream stream, MagickFormat format, string name, RequestOptions options)
		{
			await context.Guild.ModifyAsync(x => x.Splash = new Image(stream), options).CAF();
			return null;
		}
	}
}