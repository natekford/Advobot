using Advobot.Actions;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// Attempts to find a Url for an image.
	/// </summary>
	public class ImageUrlTypeReader : TypeReader 
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

			return GetActions.TryGetFileType(url, out string fileType, out ErrorReason errorReason)
				? Task.FromResult(TypeReaderResult.FromSuccess(new ImageUrl(url, fileType)))
				: Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, errorReason.Reason));
		}
	}
}
