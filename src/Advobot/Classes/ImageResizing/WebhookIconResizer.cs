using System;
using System.IO;
using System.Threading.Tasks;
using AdvorangesUtils;
using Discord;
using ImageMagick;

namespace Advobot.Classes.ImageResizing
{
	/// <summary>
	/// Used for resizing a webhook icon and changing it.
	/// </summary>
	public sealed class WebhookIconResizer : ImageResizer<IconResizerArguments>
	{
		/// <summary>
		/// Creates an instance of <see cref="WebhookIconResizer"/>.
		/// </summary>
		/// <param name="threads"></param>
		public WebhookIconResizer(int threads) : base(threads, "webhook icon") { }

		/// <inheritdoc />
		protected override async Task<Error> UseResizedImageStream(AdvobotCommandContext context, MemoryStream stream, MagickFormat format, string name, RequestOptions options)
		{
			if (!(await context.Guild.GetWebhookAsync(Convert.ToUInt64(name)).CAF() is IWebhook webhook))
			{
				return new Error("Unable to find the webhook to update.");
			}
			await webhook.ModifyAsync(x => x.Image = new Image(stream), options).CAF();
			return null;
		}
	}
}