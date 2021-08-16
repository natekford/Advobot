using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AdvorangesUtils;

using Discord;

namespace Advobot.Utilities
{
	/// <summary>
	/// Formatting for information about Discord objects.
	/// </summary>
	public static class FormattingUtils
	{
		/// <summary>
		/// Returns a new <see cref="EmbedAuthorBuilder"/> containing the user's info.
		/// </summary>
		/// <param name="author"></param>
		/// <returns></returns>
		public static EmbedAuthorBuilder CreateAuthor(this IUser author)
		{
			return new()
			{
				IconUrl = author?.GetAvatarUrl(),
				Name = author?.Format(),
				Url = author?.GetAvatarUrl(),
			};
		}

		/// <summary>
		/// Returns a string with the object's name and id.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static string Format<T>(this IEntity<T> obj) where T : IEquatable<T> => obj switch
		{
			IUser user => user.Format(),
			IRole role => role.Format(),
			IChannel channel => channel.Format(),
			IGuild guild => guild.Format(),
			IMessage message => message.Format(true),
			IActivity activity => activity.Format(),
			IWebhook webhook => webhook.Format(),
			IInviteMetadata invite => invite.Format(),
			IEmote emote => emote.Format(),
			_ => obj.ToString(),
		};

		/// <summary>
		/// Returns a string with the user's name, discriminator and id.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public static string Format(this IUser? user)
		{
			if (user is null)
			{
				return "Irretrievable User";
			}
			return $"'{user.Username.EscapeBackTicks()}#{user.Discriminator}' ({user.Id})";
		}

		/// <summary>
		/// Returns a string with the role's name and id.
		/// </summary>
		/// <param name="role"></param>
		/// <returns></returns>
		public static string Format(this IRole? role)
		{
			if (role is null)
			{
				return "Irretrievable Role";
			}
			return $"'{role.Name.EscapeBackTicks()}' ({role.Id})";
		}

		/// <summary>
		/// Returns a string with the channel's name and id.
		/// </summary>
		/// <param name="channel"></param>
		/// <returns></returns>
		public static string Format(this IChannel? channel)
		{
			if (channel is null)
			{
				return "Irretrievable Channel";
			}

			var channelType = channel switch
			{
				IMessageChannel _ => "text",
				IVoiceChannel _ => "voice",
				ICategoryChannel _ => "category",
				_ => "unknown",
			};
			return $"'{channel.Name.EscapeBackTicks()}' ({channelType}) ({channel.Id})";
		}

		/// <summary>
		/// Returns a string with the guild's name and id.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public static string Format(this IGuild? guild)
		{
			if (guild is null)
			{
				return "Irretrievable Guild";
			}
			return $"'{guild.Name.EscapeBackTicks()}' ({guild.Id})";
		}

		/// <summary>
		/// Returns a string with the messages content, embeds, and attachments listed.
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="withMentions"></param>
		/// <returns></returns>
		public static string Format(this IMessage? msg, bool withMentions)
		{
			if (msg is null)
			{
				return "Irretrievable Message";
			}

			var time = msg.CreatedAt.ToString("HH:mm:ss");
			var text = string.IsNullOrWhiteSpace(msg.Content)
				? "Empty"
				: msg.Content.EscapeBackTicks();
			var author = withMentions
				? msg.Author.Mention
				: $"{msg.Author.Format()}";
			var channel = withMentions
				? $"[Link]({msg.GetJumpUrl()})"
				: $"{msg.Channel.Format()} ({msg.Id})";
			var sb = new StringBuilder($"{author} {channel} `[{time}]`\n```{text}");

			var currentEmbed = 1;
			foreach (var embed in msg.Embeds)
			{
				if (embed.Description == null && embed.Url == null && !embed.Image.HasValue)
				{
					continue;
				}

				var description = embed.Description?.EscapeBackTicks() ?? "No description";
				sb.Append("Embed ").Append(currentEmbed).Append(": ").Append(description);
				if (embed.Url != null)
				{
					sb.Append(" URL: ").Append(embed.Url);
				}
				if (embed.Image.HasValue)
				{
					sb.Append(" IURL: ").Append(embed.Image.Value.Url);
				}
				sb.AppendLineFeed();
				++currentEmbed;
			}

			var attachments = msg.Attachments.Join(x => x.Filename, " + ");
			if (!string.IsNullOrWhiteSpace(attachments))
			{
				sb.AppendLineFeed($" + {attachments.EscapeBackTicks()}");
			}
			return sb.Append("```").ToString();
		}

		/// <summary>
		/// Returns a string with the game's name or stream name/url.
		/// </summary>
		/// <param name="activity"></param>
		/// <returns></returns>
		public static string Format(this IActivity? activity) => activity switch
		{
			CustomStatusGame csg => csg.State,
			SpotifyGame sp => $"Listening to {sp.TrackTitle}",
			StreamingGame sg => $"Streaming {sg.Name.EscapeBackTicks()} at {sg.Url}",
			RichGame rg => $"Playing {rg.Name.EscapeBackTicks()} ({rg.State.EscapeBackTicks()})",
			Game g => $"Playing {g.Name.EscapeBackTicks()}",
			_ => "N/A",
		};

		/// <summary>
		/// Returns a string with the webhook's name and id.
		/// </summary>
		/// <param name="webhook"></param>
		/// <returns></returns>
		public static string Format(this IWebhook? webhook)
		{
			if (webhook is null)
			{
				return "Irretrievable Webhook";
			}
			return $"'{webhook.Name.EscapeBackTicks()}' ({webhook.Id})";
		}

		/// <summary>
		/// Formats information about an invite.
		/// </summary>
		/// <param name="invite"></param>
		/// <returns></returns>
		public static string Format(this IInviteMetadata? invite)
		{
			if (invite is null)
			{
				return "Irretrievable Invite";
			}

			const string INF = "\u221E"; //∞
			var uses = invite.MaxUses.HasValue ? invite.MaxUses.Value.ToString() : INF;
			var time = invite.MaxAge.HasValue ? (invite.MaxAge.Value / 60).ToString() : INF;
			var temp = invite.IsTemporary ? ", temp" : "";
			return $"'{invite.Code}' ({uses} uses, {time} minutes{temp})";
		}

		/// <summary>
		/// Formats information about an emote.
		/// </summary>
		/// <param name="emote"></param>
		/// <returns></returns>
		public static string Format(this IEmote emote) => emote switch
		{
			Emote e => $"'{e.Name.EscapeBackTicks()}' ({e.Id})",
			IEmote e => $"'{e.Name.EscapeBackTicks()}'",
			_ => "Irretrievable Emote",
		};

		/// <summary>
		/// Invokes <see cref="string.Format(string, object[])"/>.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static string Format(this string format, params MarkdownFormattedArg[] args)
		{
			var casted = Array.ConvertAll(args, x => x.Value);
			return string.Format(format, casted);
		}

		/// <summary>
		/// Formats the interpolated string with the specified format provider.
		/// </summary>
		/// <param name="provider"></param>
		/// <param name="formattable"></param>
		/// <returns></returns>
		public static string FormatInterpolated(
			this IFormatProvider provider,
			FormattableString formattable)
			=> formattable.ToString(provider);

		/// <summary>
		/// Formats the permissions into a precondition string.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="permissions"></param>
		/// <returns></returns>
		public static string FormatPermissions<T>(this IEnumerable<T> permissions)
			where T : Enum
		{
			return permissions.Select(x =>
			{
				var perms = default(List<string>);
				foreach (Enum e in Enum.GetValues(x.GetType()))
				{
					if (x.Equals(e))
					{
						return e.ToString();
					}
					else if (x.HasFlag(e))
					{
						perms ??= new();
						perms.Add(e.ToString());
					}
				}
				return perms.Join(" & ");
			}).Join(" | ");
		}

		/// <summary>
		/// Returns a dictionary of the names of each permission and its value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="values"></param>
		/// <param name="localizer"></param>
		/// <param name="padLength"></param>
		/// <returns></returns>
		public static IDictionary<string, string> FormatPermissionValues<T>(
			this IDictionary<T, PermValue> values,
			Func<T, string> localizer,
			out int padLength) where T : Enum
		{
			padLength = -1;
			var temp = new Dictionary<string, string>();
			foreach (var kvp in values)
			{
				var name = localizer(kvp.Key);
				var value = kvp.Value switch
				{
					PermValue.Allow => Constants.ALLOWED,
					PermValue.Deny => Constants.DENIED,
					PermValue.Inherit => Constants.INHERITED,
					_ => throw new ArgumentOutOfRangeException(nameof(kvp.Value)),
				};
				padLength = Math.Max(padLength, name.Length);
				temp.Add(name, value);
			}
			return temp;
		}

		/// <summary>
		/// Formats the string inside a big code block.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static MarkdownFormattedArg WithBigBlock(this string value)
			=> new(value, value.AddMarkdown("```"));

		/// <summary>
		/// Formats the string inside a standard code block.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static MarkdownFormattedArg WithBlock(this string value)
			=> new(value, value.AddMarkdown("`"));

		/// <summary>
		/// Returns the string as itself.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static MarkdownFormattedArg WithNoMarkdown(this string value)
			=> new(value, value);

		/// <summary>
		/// Returns the string in title case.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static MarkdownFormattedArg WithTitleCase(this string value)
			=> new(value, value.FormatTitle());

		/// <summary>
		/// Formats the string as a title (in title case, with a colon at the end, and in bold).
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static MarkdownFormattedArg WithTitleCaseAndColon(this string value)
		{
			var title = value.FormatTitle();
			if (!title.EndsWith(':'))
			{
				title += ":";
			}
			return new(value, title.AddMarkdown("**"));
		}

		private static string AddMarkdown(this string value, string markdown)
			=> markdown + value + markdown;

		/// <summary>
		/// Contains the original value and a newly formatted value.
		/// </summary>
		public readonly struct MarkdownFormattedArg
		{
			/// <summary>
			/// The original value.
			/// </summary>
			public string Original { get; }
			/// <summary>
			/// The newly created value.
			/// </summary>
			public string Value { get; }

			/// <summary>
			/// Creates an instance of <see cref="MarkdownFormattedArg"/>.
			/// </summary>
			/// <param name="original"></param>
			/// <param name="current"></param>
			public MarkdownFormattedArg(string original, string current)
			{
				Original = original;
				Value = current;
			}

			/// <inheritdoc />
			public override string ToString()
				=> Value;
		}
	}
}