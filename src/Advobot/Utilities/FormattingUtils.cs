using Discord;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Advobot.Utilities;

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
	/// Returns the input string with ` escaped.
	/// </summary>
	/// <param name="input">The input to escape backticks from.</param>
	/// <returns>The input with escaped backticks.</returns>
	[return: NotNullIfNotNull(nameof(input))]
	public static string? EscapeBackTicks(this string? input)
		=> input?.Replace("`", "\\`");

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
		_ => obj?.ToString() ?? "null",
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
			IVoiceChannel _ => "voice",
			IMessageChannel _ => "text",
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
			: msg.Author.Format();
		var channel = withMentions
			? $"[Link]({msg.GetJumpUrl()})"
			: $"{msg.Channel.Format()} ({msg.Id})";
		var sb = new StringBuilder($"{author} {channel} `[{time}]`\n```{text}");

		var currentEmbed = default(int);
		foreach (var embed in msg.Embeds)
		{
			if (embed.Description is null && embed.Url is null && !embed.Image.HasValue)
			{
				continue;
			}

			var description = embed.Description?.EscapeBackTicks() ?? "No description";
			sb.AppendLine();
			sb.Append("Embed ").Append(currentEmbed + 1).Append(": ").Append(description);
			if (embed.Url != null)
			{
				sb.Append(" URL: ").Append(embed.Url);
			}
			if (embed.Image.HasValue)
			{
				sb.Append(" IURL: ").Append(embed.Image.Value.Url);
			}
			++currentEmbed;
		}

		var attachments = msg.Attachments.Select(x => x.Filename).Join(" + ");
		if (!string.IsNullOrWhiteSpace(attachments))
		{
			sb.AppendLine().AppendLine($" + {attachments.EscapeBackTicks()}");
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
	public static string Format(this string format, params MarkdownString[] args)
	{
		var casted = Array.ConvertAll(args, x => x.Current);
		return string.Format(format, casted);
	}

	/// <summary>
	/// Returns a string which is a numbered list of the passed in strings.
	/// </summary>
	/// <param name="values">The strings to put into a numbered list.</param>
	/// <returns>A numbered list of strings.</returns>
	public static string FormatNumberedList(this IEnumerable<string> values)
	{
		var maxLen = values.Count().ToString().Length;
		return values.Select((x, index) =>
		{
			var number = (index + 1).ToString().PadLeft(maxLen, '0');
			return $"`{number}.` {x}";
		}).Join("\n");
	}

	/// <summary>
	/// Formats the permission values into a string.
	/// </summary>
	/// <param name="values"></param>
	/// <returns></returns>
	public static string FormatPermissionList(this IDictionary<string, PermValue> values)
	{
		var padLength = values.Keys.Max(x => x.Length);
		return values
			.Select(kvp =>
			{
				var emoji = kvp.Value switch
				{
					PermValue.Allow => Constants.ALLOWED,
					PermValue.Deny => Constants.DENIED,
					PermValue.Inherit => Constants.INHERITED,
					_ => throw new IndexOutOfRangeException(nameof(kvp.Value)),
				};
				return $"{kvp.Key.PadRight(padLength)} {emoji}";
			})
			.Join("\n");
	}

	/// <summary>
	/// Joins the strings together with <paramref name="separator"/>.
	/// </summary>
	/// <param name="source">The values to join.</param>
	/// <param name="separator">The value to join each string with.</param>
	/// <returns>All strings joined together.</returns>
	public static string Join(this IEnumerable<string> source, string? separator = null)
	{
		separator ??= CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";
		return string.Join(separator, source);
	}

	/// <summary>
	/// Returns the input string with carriage returns changed to new lines,
	/// no duplicate new lines, and no markdown.
	/// </summary>
	/// <param name="input">The input to remove duplicate new lines and markdown from.</param>
	/// <param name="keepMarkdown">Whether or not to keep markdown.</param>
	/// <returns>The input without any duplicate new lines or markdown.</returns>
	[return: NotNullIfNotNull(nameof(input))]
	public static string? Sanitize(this string? input, bool keepMarkdown)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		if (!keepMarkdown)
		{
			input = input.Replace("\\", "").Replace("*", "").Replace("_", "").Replace("~", "").Replace("`", "");
		}

		var str = input.Replace("\r", "\n");
		int len;
		do
		{
			len = str.Length;
			str = str.Replace("\n\n", "\n");
		} while (len != str.Length);
		return str;
	}

	/// <summary>
	/// Returns the passed in time as a human readable time.
	/// </summary>
	/// <param name="dt">The datetime to format.</param>
	/// <returns>Formatted string that is readable by humans.</returns>
	public static string ToReadable(this DateTime dt)
	{
		var utc = dt.ToUniversalTime();
		var month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(utc.Month);
		return $"{month} {utc.Day}, {utc.Year} at {utc.ToLongTimeString()}";
	}

	/// <summary>
	/// Adds in spaces between each capital letter and capitalizes every letter after a space.
	/// </summary>
	/// <param name="input">The input to put into title case.</param>
	/// <returns>The input in title case.</returns>
	public static string ToTitleCase(this string input)
	{
		var sb = new StringBuilder();
		for (var i = 0; i < input.Length; ++i)
		{
			var c = input[i];
			if (char.IsUpper(c) && i > 0 && !char.IsWhiteSpace(input[i - 1]))
			{
				sb.Append(' ');
			}
			//Determine if the char should be made capital
			sb.Append(i == 0 || (i > 0 && char.IsWhiteSpace(input[i - 1])) ? char.ToUpper(c) : c);
		}
		return sb.ToString();
	}

	/// <summary>
	/// Formats the string inside a big code block.
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public static MarkdownString WithBigBlock(this string value)
		=> new(value, value.AddMarkdown("```"));

	/// <summary>
	/// Formats the string inside a standard code block.
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public static MarkdownString WithBlock(this string value)
		=> new(value, value.AddMarkdown("`"));

	/// <summary>
	/// Returns the string as itself.
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public static MarkdownString WithNoMarkdown(this string value)
		=> new(value, value);

	/// <summary>
	/// Returns the string in title case.
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public static MarkdownString WithTitleCase(this string value)
		=> new(value, value.ToTitleCase());

	/// <summary>
	/// Formats the string as a title (in title case, with a colon at the end, and in bold).
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public static MarkdownString WithTitleCaseAndColon(this string value)
	{
		var title = value.ToTitleCase();
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
	/// <param name="Original">The original value.</param>
	/// <param name="Current">The newly created value.</param>
	public readonly record struct MarkdownString(string Original, string Current)
	{
		/// <inheritdoc />
		public override string ToString()
			=> Current;
	}
}