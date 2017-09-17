using Advobot.Interfaces;
using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Advobot.Actions
{
	public static class UploadActions
	{
		public static async Task<IUserMessage> WriteAndUploadTextFile(IGuild guild, IMessageChannel channel, string text, string fileName, string content = null)
		{
			if (!fileName.EndsWith("_"))
			{
				fileName += "_";
			}
			var fullFileName = fileName + FormattingActions.FormatDateTimeForSaving() + Constants.GENERAL_FILE_EXTENSION;
			var fileInfo = GetActions.GetServerDirectoryFile(guild.Id, fullFileName);

			//Create
			SavingAndLoadingActions.OverWriteFile(fileInfo, text.RemoveAllMarkdown());
			//Upload
			var msg = await channel.SendFileAsync(fileInfo.FullName, String.IsNullOrWhiteSpace(content) ? "" : $"**{content}:**");
			//Delete
			SavingAndLoadingActions.DeleteFile(fileInfo);
			return msg;
		}

		public static bool TryGetFileType(IMyCommandContext context, string imageUrl, out string fileType, out string errorReason)
		{
			fileType = "";
			errorReason = "";

			var req = WebRequest.Create(imageUrl);
			req.Method = WebRequestMethods.Http.Head;
			using (var resp = req.GetResponse())
			{
				if (!Constants.VALID_IMAGE_EXTENSIONS.Contains(fileType = "." + resp.Headers.Get("Content-Type").Split('/').Last()))
				{
					errorReason = "Image must be a png or jpg.";
				}
				else if (!int.TryParse(resp.Headers.Get("Content-Length"), out int ContentLength))
				{
					errorReason = "Unable to get the image's file size.";
				}
				else if (ContentLength > Constants.MAX_ICON_FILE_SIZE)
				{
					errorReason = $"Image is bigger than {(double)Constants.MAX_ICON_FILE_SIZE / 1000000:0.0}MB. Manually upload instead.";
				}
				else
				{
					return true;
				}
			}
			return false;
		}

		public static bool ValidateURL(string input)
		{
			if (String.IsNullOrWhiteSpace(input))
			{
				return false;
			}

			return Uri.TryCreate(input, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
		}
	}
}