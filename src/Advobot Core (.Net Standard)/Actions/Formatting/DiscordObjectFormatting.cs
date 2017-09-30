﻿using Advobot.Interfaces;
using Discord;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Actions.Formatting
{
	public static class DiscordObjectFormatting
    {
		/// <summary>
		/// Returns a string that better describes the object than its ToString() method.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static string FormatDiscordObject(object obj)
		{
			if (obj is IUser user)
			{
				return user.FormatUser();
			}
			else if (obj is IChannel channel)
			{
				return channel.FormatChannel();
			}
			else if (obj is IRole role)
			{
				return role.FormatRole();
			}
			else if (obj is IGuild guild)
			{
				return guild.FormatGuild();
			}
			else
			{
				return obj.ToString();
			}
		}
		/// <summary>
		/// Returns a string with the user's name, discriminator, and id.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public static string FormatUser(this IUser user)
		{
			if (user != null)
			{
				var userName = user.Username.EscapeBackTicks().CaseInsReplace("discord.gg", Constants.FAKE_DISCORD_LINK);
				return $"'{userName}#{user.Discriminator}' ({user.Id})";
			}
			else
			{
				return "Irretrievable User";
			}
		}
		/// <summary>
		/// Returns a string with the role's name and id.
		/// </summary>
		/// <param name="role"></param>
		/// <returns></returns>
		public static string FormatRole(this IRole role)
		{
			if (role != null)
			{
				return $"'{role.Name.EscapeBackTicks()}' ({role.Id})";
			}
			else
			{
				return "Irretrievable Role";
			}
		}
		/// <summary>
		/// Returns a string with the channel's name and id.
		/// </summary>
		/// <param name="channel"></param>
		/// <returns></returns>
		public static string FormatChannel(this IChannel channel)
		{
			if (channel != null)
			{
				return $"'{channel.Name.EscapeBackTicks()}' ({(channel is IMessageChannel ? "text" : "voice")}) ({channel.Id})";
			}
			else
			{
				return "Irretrievable Channel";
			}
		}
		/// <summary>
		/// Returns a string with the guild's name and id.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public static string FormatGuild(this IGuild guild)
		{
			if (guild != null)
			{
				return $"'{guild.Name.EscapeBackTicks()}' ({guild.Id})";
			}
			else
			{
				return "Irretrievable Guild";
			}
		}
		/// <summary>
		/// Returns a string of a message's content/embeds, what time it was sent at, the author, and the channel.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public static string FormatMessage(this IMessage message)
		{
			var time = message.CreatedAt.ToString("HH:mm:ss");
			var author = message.Author.FormatUser();
			var channel = message.Channel.FormatChannel();
			var text = message.FormatMessageContent().RemoveAllMarkdown().RemoveDuplicateNewLines();
			return $"`[{time}]` `{author}` **IN** `{channel}`\n```\n{text}```";
		}
		/// <summary>
		/// Replaces everyone/here mentions with a non pinging version and removes \tts.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="content"></param>
		/// <returns></returns>
		public static string FormatMessageContentForNotBeingAnnoying(IGuild guild, string content)
		{
			//Everyone and Here have the same role.
			return content
				.CaseInsReplace(guild.EveryoneRole.Mention, Constants.FAKE_EVERYONE)
				.CaseInsReplace("@everyone", Constants.FAKE_EVERYONE)
				.CaseInsReplace("@here", Constants.FAKE_HERE)
				.CaseInsReplace("\tts", Constants.FAKE_TTS);
		}

		/// <summary>
		/// Returns a string containing the message's content and then most aspects of the embeds in their messages.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public static string FormatMessageContent(this IMessage message)
		{
			var sb = new StringBuilder((String.IsNullOrEmpty(message.Content) ? "Empty message content" : message.Content) + "\n");

			if (message.Embeds.Any())
			{
				var validEmbeds = message.Embeds.Where(x => x.Description != null || x.Url != null || x.Image.HasValue);
				var formattedDescriptions = validEmbeds.Select((x, index) =>
				{
					var tempSb = new StringBuilder($"Embed {index + 1}: {x.Description ?? "No description"}");
					if (x.Url != null)
					{
						tempSb.Append($" URL: {x.Url}");
					}
					if (x.Image.HasValue)
					{
						tempSb.Append($" IURL: {x.Image.Value.Url}");
					}
					return tempSb.ToString();
				});

				sb.AppendLineFeed(String.Join("\n", formattedDescriptions));
			}
			if (message.Attachments.Any())
			{
				sb.Append(" + " + String.Join(" + ", message.Attachments.Select(x => x.Filename)));
			}

			return sb.ToString();
		}
		/// <summary>
		/// Returns the game's name or stream name/url.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public static string FormatGame(IUser user)
		{
			var game = user.Game.Value;
			switch (game.StreamType)
			{
				case StreamType.NotStreaming:
				{
					return $"**Current Game:** `{game.Name.EscapeBackTicks()}`";
				}
				case StreamType.Twitch:
				{
					return $"**Current Stream:** [{game.Name.EscapeBackTicks()}]({game.StreamUrl})";
				}
				default:
				{
					return "**Current Game:** `N/A`";
				}
			}
		}
		/// <summary>
		/// Returns a string which is a human readable stay time.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public static string FormatStayLength(IGuildUser user)
		{
			if (user.JoinedAt.HasValue)
			{
				var timeStayed = (DateTime.UtcNow - user.JoinedAt.Value.ToUniversalTime());
				return $"**Stayed for:** {timeStayed.Days}:{timeStayed.Hours:00}:{timeStayed.Minutes:00}:{timeStayed.Seconds:00}";
			}
			return "";
		}
		/// <summary>
		/// Returns a string detailing which invite a user joined on.
		/// </summary>
		/// <param name="guildSettings"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		public static async Task<string> FormatInviteJoin(IGuildSettings guildSettings, IGuildUser user)
		{
			var curInv = await InviteActions.GetInviteUserJoinedOn(guildSettings, user);
			return curInv != null ? $"**Invite:** {curInv.Code}" : "";
		}
		/// <summary>
		/// Returns a string which warns about young accounts.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public static string FormatAccountAgeWarning(IUser user)
		{
			var userAccAge = (DateTime.UtcNow - user.CreatedAt.ToUniversalTime());
			if (userAccAge.TotalHours < 24)
			{
				return $"**New Account:** {(int)userAccAge.TotalHours} hours, {userAccAge.Minutes} minutes old.";
			}
			return "";
		}
	}
}