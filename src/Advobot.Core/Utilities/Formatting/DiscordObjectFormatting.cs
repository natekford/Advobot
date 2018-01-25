using System;
using System.Linq;
using System.Text;
using Discord;

namespace Advobot.Core.Utilities.Formatting
{
	/// <summary>
	/// Formatting for various Discord objects.
	/// </summary>
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
				return user.Format();
			}

			if (obj is IChannel channel)
			{
				return channel.Format();
			}

			if (obj is IRole role)
			{
				return role.Format();
			}

			if (obj is IGuild guild)
			{
				return guild.Format();
			}

			if (obj is IActivity presence)
			{
				return presence.Format();
			}

			return obj.ToString();
		}
		/// <summary>
		/// Returns a string with the user's name, discriminator, and id.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public static string Format(this IUser user)
		{
			if (user != null)
			{
				var username = user.Username.EscapeBackTicks()
					.CaseInsReplace("discord.gg", Constants.FAKE_DISCORD_LINK);
				return $"'{username}#{user.Discriminator}' ({user.Id})";
			}
			return "Irretrievable User";
		}
		/// <summary>
		/// Returns a string with the role's name and id.
		/// </summary>
		/// <param name="role"></param>
		/// <returns></returns>
		public static string Format(this IRole role)
		{
			return role != null
				? $"'{role.Name.EscapeBackTicks()}' ({role.Id})"
				: "Irretrievable Role";
		}
		/// <summary>
		/// Returns a string with the channel's name and id.
		/// </summary>
		/// <param name="channel"></param>
		/// <returns></returns>
		public static string Format(this IChannel channel)
		{
			if (channel != null)
			{
				var type = channel is IMessageChannel ? "text" : "voice";
				return $"'{channel.Name.EscapeBackTicks()}' ({type}) ({channel.Id})";
			}
			return "Irretrievable Channel";
		}
		/// <summary>
		/// Returns a string with the guild's name and id.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public static string Format(this IGuild guild)
		{
			return guild != null
				? $"'{guild.Name.EscapeBackTicks()}' ({guild.Id})"
				: "Irretrievable Guild";
		}
		/// <summary>
		/// Returns a string with the messages content, embeds, and attachments listed.
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="withMentions"></param>
		/// <returns></returns>
		public static string Format(this IMessage msg, bool withMentions)
		{
			var embeds = msg.Embeds.Where(x => x.Description != null || x.Url != null || x.Image.HasValue).Select((x, index) =>
			{
				var embed = new StringBuilder($"Embed {index + 1}: {x.Description ?? "No description"}");
				if (x.Url != null)
				{
					embed.Append($" URL: {x.Url}");
				}
				if (x.Image.HasValue)
				{
					embed.Append($" IURL: {x.Image.Value.Url}");
				}
				return embed.ToString();
			});
			var attachments = msg.Attachments.Select(x => x.Filename).ToList();

			var text = String.IsNullOrEmpty(msg.Content) ? "Empty message content" : msg.Content;
			var time = msg.CreatedAt.ToString("HH:mm:ss");

			string header;
			if (withMentions)
			{
				var userMention = msg.Author.Mention;
				var channelMention = ((ITextChannel)msg.Channel).Mention;
				header = $"`[{time}]` {userMention} IN {channelMention} `{msg.Id}`";
			}
			else
			{
				var user = msg.Author.Format();
				var channel = msg.Channel.Format();
				header = $"`[{time}]` `{user}` IN `{channel}` `{msg.Id}`";
			}

			var content = new StringBuilder($"{header}\n```\n{text.EscapeBackTicks()}");
			foreach (var embed in embeds)
			{
				content.AppendLineFeed(embed.EscapeBackTicks());
			}
			if (attachments.Any())
			{
				content.AppendLineFeed($" + {String.Join(" + ", attachments).EscapeBackTicks()}");
			}
			return content.Append("```").ToString();
		}
		/// <summary>
		/// Returns the game's name or stream name/url.
		/// </summary>
		/// <param name="presence"></param>
		/// <returns></returns>
		public static string Format(this IActivity presence)
		{
			if (presence is StreamingGame sg)
			{
				return $"**Current Stream:** [{sg.Name.EscapeBackTicks()}]({sg.Url})";
			}

			if (presence is Game g)
			{
				return $"**Current Game:** `{g.Name.EscapeBackTicks()}`";
			}

			return "**Current Game:** `N/A`";
		}
		/// <summary>
		/// Returns the channel perms gotten from the filtered overwrite permissions formatted with their perm value in front of the perm name.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="channel"></param>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static string FormatOverwritePerms<T>(this IGuildChannel channel, T obj) where T : ISnowflakeEntity
		{
			var overwrite = channel.PermissionOverwrites.FirstOrDefault(x => x.TargetId == obj.Id);
			var perms = OverwriteUtils.GetChannelOverwriteNamesAndValues(overwrite, channel);
			var maxLen = perms.Keys.Max(x => x.Length);
			return String.Join("\n", perms.Select(x => $"{x.Key.PadRight(maxLen)} {x.Value}"));
		}
		/// <summary>
		/// Replaces everyone/here mentions with a non pinging version and removes \tts.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="content"></param>
		/// <returns></returns>
		public static string FormatMessageContent(IGuild guild, string content)
		{
			return content
				.CaseInsReplace(guild.EveryoneRole.Mention, Constants.FAKE_EVERYONE) //Everyone and Here have the same role.
				.CaseInsReplace("@everyone", Constants.FAKE_EVERYONE)
				.CaseInsReplace("@here", Constants.FAKE_HERE)
				.CaseInsReplace("\tts", Constants.FAKE_TTS);
		}
	}
}
