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
			SavingAndLoadingActions.OverWriteFile(fileInfo, text.RemoveAllMarkdown());

			var msg = await UploadFile(channel, fileInfo, String.IsNullOrWhiteSpace(content) ? "" : $"**{content}:**");

			SavingAndLoadingActions.DeleteFile(fileInfo);
			return msg;
		}
		public static async Task<IUserMessage> UploadFile(IMessageChannel channel, FileInfo fileInfo, string text = null)
		{
			return await channel.SendFileAsync(fileInfo.FullName, text);
		}

		public static async Task SetBotIcon(IMyCommandContext context, string imageURL)
		{
			if (imageURL == null)
			{
				await context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image());
				await MessageActions.MakeAndDeleteSecondaryMessage(context, "Successfully removed the bot's icon.");
				return;
			}

			var fileType = await GetFileTypeOrSayErrors(context, imageURL);
			if (fileType == null)
				return;

			var fileInfo = GetActions.GetServerDirectoryFile(context.Guild.Id, Constants.BOT_ICON_LOCATION + fileType);
			using (var webclient = new WebClient())
			{
				webclient.DownloadFileAsync(new Uri(imageURL), fileInfo.FullName);
				webclient.DownloadFileCompleted += async (sender, e) => await SetIcon(sender, e, context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(fileInfo.FullName)), context, fileInfo);
			}
		}
		public static async Task<string> GetFileTypeOrSayErrors(IMyCommandContext context, string imageURL)
		{
			string fileType;
			var req = WebRequest.Create(imageURL);
			req.Method = WebRequestMethods.Http.Head;
			using (var resp = req.GetResponse())
			{
				if (!Constants.VALID_IMAGE_EXTENSIONS.Contains(fileType = "." + resp.Headers.Get("Content-Type").Split('/').Last()))
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR("Image must be a png or jpg."));
					return null;
				}
				else if (!int.TryParse(resp.Headers.Get("Content-Length"), out int ContentLength))
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR("Unable to get the image's file size."));
					return null;
				}
				else if (ContentLength > Constants.MAX_ICON_FILE_SIZE)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR($"Image is bigger than {(double)Constants.MAX_ICON_FILE_SIZE / 1000000:0.0}MB. Manually upload instead."));
					return null;
				}
			}
			return fileType;
		}
		public static async Task SetIcon(object sender, System.ComponentModel.AsyncCompletedEventArgs e, Task iconSetter, IMyCommandContext context, FileInfo fileInfo)
		{
			await iconSetter.ContinueWith(async prevTask =>
			{
				if (prevTask?.Exception?.InnerExceptions?.Any() ?? false)
				{
					var exceptionMessages = new List<string>();
					foreach (var exception in prevTask.Exception.InnerExceptions)
					{
						ConsoleActions.ExceptionToConsole(exception);
						exceptionMessages.Add(exception.Message);
					}
					await MessageActions.SendChannelMessage(context, $"Failed to change the bot icon. Following exceptions occurred:\n{String.Join("\n", exceptionMessages)}.");
				}
				else
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(context, "Successfully changed the bot icon.");
				}

				SavingAndLoadingActions.DeleteFile(fileInfo);
			});
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