using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Advobot.Core.Classes;
using Advobot.Core.Classes.UserInformation;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Core.Classes.Punishments;

namespace Advobot.Core.Services.Log
{
	internal sealed class MessageHandler
	{
		private static Dictionary<SpamType, Func<IMessage, int?>> _GetSpamNumberFuncs = new Dictionary<SpamType, Func<IMessage, int?>>
		{
			{ SpamType.Message,     (message) => int.MaxValue },
			{ SpamType.LongMessage, (message) => message.Content?.Length },
			{ SpamType.Link,        (message) => message.Content?.Split(' ')?.Count(x => Uri.IsWellFormedUriString(x, UriKind.Absolute)) },
			{ SpamType.Image,       (message) => message.Attachments.Where(x => x.Height != null || x.Width != null).Count() + message.Embeds.Where(x => x.Image != null || x.Video != null).Count() },
			{ SpamType.Mention,     (message) => message.MentionedUserIds.Distinct().Count() },
		};

		private LogInstance _LogInstance;
		private ITimersService _Timers;
		private ILogService _Logging;
		private bool _IsValid;

		public MessageHandler(LogInstance instance, ITimersService timers, ILogService logging)
		{
			_LogInstance = instance;
			_Timers = timers;
			_Logging = logging;
			_IsValid = instance.Message != null && instance.User != null && instance.Channel != null && instance.Guild != null && instance.GuildSettings != null;
		}

		/// <summary>
		/// Handles settings on channels, such as: image only mode.
		/// </summary>
		/// <returns></returns>
		public async Task HandleChannelSettingsAsync()
		{
			if (!_IsValid)
			{
				return;
			}

			if (true
				&& !_LogInstance.User.GuildPermissions.Administrator
				&& _LogInstance.GuildSettings.ImageOnlyChannels.Contains(_LogInstance.Channel.Id)
				&& !_LogInstance.Message.Attachments.Any(x => x.Height != null || x.Width != null)
				&& !_LogInstance.Message.Embeds.Any(x => x.Image != null))
			{
				await _LogInstance.Message.DeleteAsync().CAF();
			}
		}
		/// <summary>
		/// Logs the image to the image log if set.
		/// </summary>
		/// <returns></returns>
		public async Task HandleImageLoggingAsync()
		{
			if (!_IsValid)
			{
				return;
			}

			var attachmentURLs = _LogInstance.Message.Attachments.Select(x => x.Url).Distinct();
			var embedURLs = new List<string>();
			var videoEmbeds = new List<IEmbed>();

			foreach (var embed in _LogInstance.Message.Embeds)
			{
				if (embed.Video == null)
				{
					//If no video then it has to be just an image
					if (!String.IsNullOrEmpty(embed.Thumbnail?.Url))
					{
						embedURLs.Add(embed.Thumbnail?.Url);
					}
					if (!String.IsNullOrEmpty(embed.Image?.Url))
					{
						embedURLs.Add(embed.Image?.Url);
					}
				}
				else
				{
					//Add the video URL and the thumbnail URL
					videoEmbeds.Add(embed);
				}
			}

			var desc = $"**Channel:** `{_LogInstance.Channel.FormatChannel()}`\n**Message Id:** `{_LogInstance.Message.Id}`";
			foreach (var attachmentURL in attachmentURLs) //Attachments
			{
				if (Constants.VALID_IMAGE_EXTENSIONS.CaseInsContains(Path.GetExtension(attachmentURL))) //Image
				{
					_Logging.Images.Increment();
					var embed = new EmbedWrapper(null, desc, Constants.ATCH, attachmentURL)
						.AddAuthor(_LogInstance.User, attachmentURL)
						.AddFooter("Attached Image");
					await MessageUtils.SendEmbedMessageAsync(_LogInstance.GuildSettings.ImageLog, embed).CAF();
				}
				else if (Constants.VALID_GIF_EXTENTIONS.CaseInsContains(Path.GetExtension(attachmentURL))) //Gif
				{
					_Logging.Gifs.Increment();
					var embed = new EmbedWrapper(null, desc, Constants.ATCH, attachmentURL)
						.AddAuthor(_LogInstance.User, attachmentURL)
						.AddFooter("Attached Gif");
					await MessageUtils.SendEmbedMessageAsync(_LogInstance.GuildSettings.ImageLog, embed).CAF();
				}
				else //Random file
				{
					_Logging.Files.Increment();
					var embed = new EmbedWrapper(null, desc, Constants.ATCH, attachmentURL)
						.AddAuthor(_LogInstance.User, attachmentURL)
						.AddFooter("Attached File");
					await MessageUtils.SendEmbedMessageAsync(_LogInstance.GuildSettings.ImageLog, embed).CAF();
				}
			}
			foreach (var embedURL in embedURLs.Distinct()) //Images
			{
				_Logging.Images.Increment();
				var embed = new EmbedWrapper(null, desc, Constants.ATCH, embedURL)
					.AddAuthor(_LogInstance.User, embedURL)
					.AddFooter("Embedded Image");
				await MessageUtils.SendEmbedMessageAsync(_LogInstance.GuildSettings.ImageLog, embed).CAF();
			}
			foreach (var videoEmbed in videoEmbeds.GroupBy(x => x.Url).Select(x => x.First())) //Videos/Gifs
			{
				_Logging.Gifs.Increment();
				var embed = new EmbedWrapper(null, desc, Constants.ATCH, videoEmbed.Thumbnail?.Url)
					.AddAuthor(_LogInstance.User, videoEmbed.Url)
					.AddFooter("Embedded " + (Constants.VALID_GIF_EXTENTIONS.CaseInsContains(Path.GetExtension(videoEmbed.Thumbnail?.Url)) ? "Gif" : "Video"));
				await MessageUtils.SendEmbedMessageAsync(_LogInstance.GuildSettings.ImageLog, embed).CAF();
			}
		}
		/// <summary>
		/// Checks the message against the slowmode.
		/// </summary>
		/// <returns></returns>
		public async Task HandleSlowmodeAsync()
		{
			if (!_IsValid)
			{
				return;
			}

			//Don't bother doing stuff on the user if they're immune
			var slowmode = _LogInstance.GuildSettings.Slowmode;
			if (slowmode == null || !slowmode.Enabled || _LogInstance.User.RoleIds.Intersect(slowmode.ImmuneRoleIds).Any())
			{
				return;
			}

			var info = _Timers.GetSlowmodeUser(_LogInstance.User);
			if (info == null)
			{
				_Timers.AddSlowmodeUser(info = new SlowmodeUserInfo(_LogInstance.User, slowmode.BaseMessages, slowmode.Interval));
			}

			if (info.CurrentMessagesLeft > 0)
			{
				if (info.CurrentMessagesLeft == slowmode.BaseMessages)
				{
					info.UpdateTime(slowmode.Interval);
				}

				info.DecrementMessages();
			}
			else
			{
				await MessageUtils.DeleteMessageAsync(_LogInstance.Message, new ModerationReason("slowmode")).CAF();
			}
		}
		/// <summary>
		/// If the message author can be modified by the bot then their message is checked for any spam matches.
		/// Then checks if there are any user mentions in thier message for voting on user kicks.
		/// </summary>
		/// <returns></returns>
		public async Task HandleSpamPreventionAsync()
		{
			if (!_IsValid)
			{
				return;
			}

			if (_LogInstance.User.Guild.GetBot().GetIfCanModifyUser(_LogInstance.User))
			{
				var spamUser = _Timers.GetSpamPreventionUser(_LogInstance.User);
				if (spamUser == null)
				{
					_Timers.AddSpamPreventionUser(spamUser = new SpamPreventionUserInfo(_LogInstance.User));
				}

				var spam = false;
				foreach (SpamType type in Enum.GetValues(typeof(SpamType)))
				{
					var prev = _LogInstance.GuildSettings.SpamPreventionDictionary[type];
					if (prev == null || !prev.Enabled)
					{
						continue;
					}

					var spamAmount = _GetSpamNumberFuncs[type](_LogInstance.Message) ?? 0;
					if (spamAmount >= prev.RequiredSpamPerMessageOrTimeInterval)
					{
						spamUser.AddSpamInstance(type, _LogInstance.Message);
					}
					if (spamUser.GetSpamAmount(type, prev.RequiredSpamPerMessageOrTimeInterval) < prev.RequiredSpamInstances)
					{
						continue;
					}

					//Make sure they have the lowest vote count required to kick and the most severe punishment type
					spamUser.VotesRequired = prev.VotesForKick;
					spamUser.Punishment = prev.PunishmentType;
					spam = true;
				}

				if (spam)
				{
					var votesReq = spamUser.VotesRequired - spamUser.Votes;
					var content = $"The user `{_LogInstance.User.FormatUser()}` needs `{votesReq}` votes to be kicked. Vote by mentioning them.";
					var channel = _LogInstance.Channel as ITextChannel;
					await MessageUtils.MakeAndDeleteSecondaryMessageAsync(channel, null, content, 10, _Timers).CAF();
					await MessageUtils.DeleteMessageAsync(_LogInstance.Message, new ModerationReason("spam prevention")).CAF();
				}
			}
			if (!_LogInstance.Message.MentionedUserIds.Any())
			{
				return;
			}

			//Get the users who are able to be punished by the spam prevention
			var users = _Timers.GetSpamPreventionUsers(_LogInstance.Guild).Where(x =>
			{
				return x.PotentialPunishment
					&& x.User.Id != _LogInstance.User.Id
					&& _LogInstance.Message.MentionedUserIds.Contains(x.User.Id)
					&& !x.HasUserAlreadyVoted(_LogInstance.User.Id);
			});
			if (!users.Any())
			{
				return;
			}

			var giver = new PunishmentGiver(0, null);
			var reason = new ModerationReason("spam prevention");
			foreach (var u in users)
			{
				u.IncreaseVotes(_LogInstance.User.Id);
				if (u.Votes < u.VotesRequired)
				{
					return;
				}

				await giver.PunishAsync(u.Punishment, u.User, _LogInstance.GuildSettings.MuteRole, reason).CAF();

				//Reset their current spam count and the people who have already voted on them so they don't get destroyed instantly if they join back
				u.Reset();
			}
		}
		/// <summary>
		/// Makes sure a message doesn't have any banned phrases.
		/// </summary>
		/// <returns></returns>
		public async Task HandleBannedPhrasesAsync()
		{
			if (!_IsValid)
			{
				return;
			}

			//Ignore admins and messages older than an hour. (Accidentally deleted something important once due to not having these checks in place, but this should stop most accidental deletions)
			if (false
				|| _LogInstance.User.GuildPermissions.Administrator
				|| (int)DateTime.UtcNow.Subtract(_LogInstance.Message.CreatedAt.UtcDateTime).TotalHours > 0)
			{
				return;
			}

			var str = _LogInstance.GuildSettings.BannedPhraseStrings.FirstOrDefault(x =>
				_LogInstance.Message.Content.CaseInsContains(x.Phrase));
			if (str != null)
			{
				await str.PunishAsync(_LogInstance.GuildSettings, _LogInstance.Message, _Timers).CAF();
				return;
			}

			var regex = _LogInstance.GuildSettings.BannedPhraseRegex.FirstOrDefault(x =>
				RegexUtils.CheckIfRegexMatch(_LogInstance.Message.Content, x.Phrase));
			if (regex != null)
			{
				await regex.PunishAsync(_LogInstance.GuildSettings, _LogInstance.Message, _Timers).CAF();
				return;
			}
		}
	}
}