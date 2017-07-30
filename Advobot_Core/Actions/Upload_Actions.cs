using Advobot.Interfaces;
using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Advobot
{
	namespace Actions
	{
		public static class UploadActions
		{
			public static async Task<IUserMessage> WriteAndUploadTextFile(IGuild guild, IMessageChannel channel, string text, string fileName, string content = null)
			{
				//Get the file path
				if (!fileName.EndsWith("_"))
				{
					fileName += "_";
				}

				var file = fileName + FormattingActions.FormatDateTimeForSaving() + Constants.GENERAL_FILE_EXTENSION;
				var path = GetActions.GetServerFilePath(guild.Id, file);
				if (path == null)
					return null;

				using (var writer = new StreamWriter(path))
				{
					writer.WriteLine(FormattingActions.RemoveMarkdownChars(text, false));
				}

				var textOnTop = String.IsNullOrWhiteSpace(content) ? "" : String.Format("**{0}:**", content);
				var msg = await channel.SendFileAsync(path, textOnTop);
				File.Delete(path);
				return msg;
			}
			public static async Task UploadFile(IMessageChannel channel, string path, string text = null)
			{
				await channel.SendFileAsync(path, text);
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

				var path = GetActions.GetServerFilePath(context.Guild.Id, Constants.BOT_ICON_LOCATION + fileType);
				using (var webclient = new WebClient())
				{
					webclient.DownloadFileAsync(new Uri(imageURL), path);
					webclient.DownloadFileCompleted += async (sender, e) => await SetIcon(sender, e, context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(path)), context, path);
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
						await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR(String.Format("Image is bigger than {0:0.0}MB. Manually upload instead.", (double)Constants.MAX_ICON_FILE_SIZE / 1000000)));
						return null;
					}
				}
				return fileType;
			}
			public static async Task SetIcon(object sender, System.ComponentModel.AsyncCompletedEventArgs e, Task iconSetter, IMyCommandContext context, string path)
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
						await MessageActions.SendChannelMessage(context, String.Format("Failed to change the bot icon. Following exceptions occurred:\n{0}.", String.Join("\n", exceptionMessages)));
					}
					else
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(context, "Successfully changed the bot icon.");
					}

					File.Delete(path);
				});
			}

			public static bool ValidateURL(string input)
			{
				if (input == null)
					return false;

				return Uri.TryCreate(input, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
			}
		}
	}
}