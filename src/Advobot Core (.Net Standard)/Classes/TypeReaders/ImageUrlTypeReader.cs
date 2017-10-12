using Discord.Commands;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// Attempts to find a Url for an image.
	/// </summary>
	public sealed class ImageUrlTypeReader : TypeReader 
	{
		/// <summary>
		/// Checks if the input is a valid Url, otherwise checks attached images and embedded images.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
		{
			var url = input;
			string fileType;

			if (url != null)
			{
				if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
				{
					return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid Url provided."));
				}
			}

			if (url == null)
			{
				var attach = context.Message.Attachments.Where(x => x.Width != null && x.Height != null).Select(x => x.Url);
				var embeds = context.Message.Embeds.Where(x => x.Image.HasValue).Select(x => x.Image?.Url);
				var imageUrls = attach.Concat(embeds);
				if (!imageUrls.Any())
				{
					return Task.FromResult(TypeReaderResult.FromSuccess(new ImageUrl(null, null)));
				}
				else if (imageUrls.Count() == 1)
				{
					url = imageUrls.First();
				}
				else
				{
					return Task.FromResult(TypeReaderResult.FromError(CommandError.MultipleMatches, "Too many attached or embedded images."));
				}
			}

			var req = WebRequest.Create(url);
			req.Method = WebRequestMethods.Http.Head;
			using (var resp = req.GetResponse())
			{
				if (!Constants.VALID_IMAGE_EXTENSIONS.Contains(fileType = "." + resp.Headers.Get("Content-Type").Split('/').Last()))
				{
					return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Image must be a png or jpg."));
				}
				else if (!int.TryParse(resp.Headers.Get("Content-Length"), out int ContentLength))
				{
					return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Unable to get the image's file size."));
				}
				else if (ContentLength > Constants.MAX_ICON_FILE_SIZE)
				{
					var maxSize = (double)Constants.MAX_ICON_FILE_SIZE / 1000 * 1000;
					return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"Image is bigger than {maxSize:0.0}MB. Manually upload instead."));
				}
				else
				{
					return Task.FromResult(TypeReaderResult.FromSuccess(new ImageUrl(url, fileType)));
				}
			}
		}
	}
}
