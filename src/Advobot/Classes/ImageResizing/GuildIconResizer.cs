using System.IO;
using System.Threading.Tasks;
using AdvorangesUtils;
using Discord;
using ImageMagick;

namespace Advobot.Classes.ImageResizing
{
	/// <summary>
	/// Used for resizing a guild icon and changing it.
	/// </summary>
	public sealed class GuildIconResizer : ImageResizer<IconResizerArguments>
	{
		/// <summary>
		/// Creates an instance of <see cref="GuildIconResizer"/>.
		/// </summary>
		/// <param name="threads"></param>
		public GuildIconResizer(int threads) : base(threads, "guild icon") { }

		/// <inheritdoc />
		protected override async Task<Error> UseResizedImageStream(AdvobotSocketCommandContext context, MemoryStream stream, MagickFormat format, string name, RequestOptions options)
		{
			await context.Guild.ModifyAsync(x => x.Icon = new Image(stream), options).CAF();
			return null;
		}
	}
}