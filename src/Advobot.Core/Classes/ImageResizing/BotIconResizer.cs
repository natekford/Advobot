using System.IO;
using System.Threading.Tasks;
using AdvorangesUtils;
using Discord;
using ImageMagick;

namespace Advobot.Classes.ImageResizing
{
	/// <summary>
	/// Used for resizing a bot icon and changing it.
	/// </summary>
	public sealed class BotIconResizer : ImageResizer<IconResizerArguments>
	{
		/// <summary>
		/// Creates an instance of <see cref="BotIconResizer"/>.
		/// </summary>
		/// <param name="threads"></param>
		public BotIconResizer(int threads) : base(threads, "bot icon") { }

		/// <inheritdoc />
		protected override async Task<Error> UseResizedImageStream(AdvobotCommandContext context, MemoryStream stream, MagickFormat format, string name, RequestOptions options)
		{
			await context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(stream), options).CAF();
			return null;
		}
	}
}