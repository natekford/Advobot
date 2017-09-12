using Discord;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Advobot.Actions
{
	public static class EmbedActions
	{
		public static EmbedBuilder MakeNewEmbed(string title = null, string description = null, Color? color = null, string imageUrl = null, string url = null, string thumbnailUrl = null, string prefix = Constants.BOT_PREFIX)
		{
			imageUrl = UploadActions.ValidateURL(imageUrl) ? imageUrl : null;
			url = UploadActions.ValidateURL(url) ? url : null;
			thumbnailUrl = UploadActions.ValidateURL(thumbnailUrl) ? thumbnailUrl : null;

			var embed = new EmbedBuilder().WithColor(Constants.BASE);

			//Add in the properties
			if (title != null)
			{
				embed.WithTitle(title.Substring(0, Math.Min(Constants.MAX_TITLE_LENGTH, title.Length)));
			}
			if (description != null)
			{
				try
				{
					embed.WithDescription(description.Replace(Constants.BOT_PREFIX, prefix));
				}
				catch (Exception e)
				{
					ConsoleActions.ExceptionToConsole(e);
				}
			}
			if (color != null)
			{
				embed.WithColor(color.Value);
			}
			if (imageUrl != null)
			{
				embed.WithImageUrl(imageUrl);
			}
			if (url != null)
			{
				embed.WithUrl(url);
			}
			if (thumbnailUrl != null)
			{
				embed.WithThumbnailUrl(thumbnailUrl);
			}

			return embed;
		}
		public static EmbedBuilder AddAuthor(EmbedBuilder embed, string name = null, string iconUrl = null, string url = null)
		{
			iconUrl = UploadActions.ValidateURL(iconUrl) ? iconUrl : null;
			url = UploadActions.ValidateURL(url) ? url : null;
			embed.WithAuthor(x =>
			{
				if (name != null)
				{
					x.Name = name.Substring(0, Math.Min(Constants.MAX_TITLE_LENGTH, name.Length));
				}
				if (iconUrl != null)
				{
					x.IconUrl = iconUrl;
				}
				if (url != null)
				{
					x.Url = url;
				}
			});
			return embed;
		}
		public static EmbedBuilder AddAuthor(EmbedBuilder embed, IUser user, string URL = null)
		{
			return AddAuthor(embed, user.Username, user.GetAvatarUrl(), URL ?? user.GetAvatarUrl());
		}
		public static EmbedBuilder AddFooter(EmbedBuilder embed, [CallerMemberName] string text = null, string iconUrl = null)
		{
			iconUrl = UploadActions.ValidateURL(iconUrl) ? iconUrl : null;
			embed.WithFooter(x =>
			{
				if (text != null)
				{
					x.Text = text.Substring(0, Math.Min(Constants.MAX_FOOTER_LENGTH, text.Length));
				}
				if (iconUrl != null)
				{
					x.IconUrl = iconUrl;
				}
			});
			return embed;
		}
		public static EmbedBuilder AddField(EmbedBuilder embed, string name, string value, bool isInline = true, string prefix = Constants.BOT_PREFIX)
		{
			if (embed.Build().Fields.Count() >= Constants.MAX_FIELDS)
			{
				return embed;
			}

			name = String.IsNullOrWhiteSpace(name) ? "Placeholder" : name.Substring(0, Math.Min(Constants.MAX_FIELD_NAME_LENGTH, name.Length));
			value = String.IsNullOrWhiteSpace(name) ? "Placeholder" : value.Replace(Constants.BOT_PREFIX, prefix).Substring(0, Math.Min(Constants.MAX_FIELD_VALUE_LENGTH, value.Length));
			embed.AddField(x =>
			{
				x.Name = name;
				x.Value = value;
				x.IsInline = isInline;
			});
			return embed;
		}
	}
}