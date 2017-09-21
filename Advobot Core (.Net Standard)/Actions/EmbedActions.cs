using Discord;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Advobot.Actions
{
	public static class EmbedActions
	{
		/// <summary>
		/// Create a new <see cref="EmbedBuilder"/> with the given parameters. Verifies Urls exist and cuts things to appropriate lengths.
		/// </summary>
		/// <param name="title"></param>
		/// <param name="description"></param>
		/// <param name="color"></param>
		/// <param name="imageUrl"></param>
		/// <param name="url"></param>
		/// <param name="thumbnailUrl"></param>
		/// <param name="prefix"></param>
		/// <returns></returns>
		public static EmbedBuilder MakeNewEmbed(string title = null, string description = null, Color? color = null, string imageUrl = null,
			string url = null, string thumbnailUrl = null)
		{
			imageUrl = GetActions.GetIfStringIsValidUrl(imageUrl) ? imageUrl : null;
			url = GetActions.GetIfStringIsValidUrl(url) ? url : null;
			thumbnailUrl = GetActions.GetIfStringIsValidUrl(thumbnailUrl) ? thumbnailUrl : null;

			var embed = new EmbedBuilder().WithColor(Colors.BASE);
			if (title != null)
			{
				embed.WithTitle(title.Substring(0, Math.Min(Constants.MAX_TITLE_LENGTH, title.Length)));
			}
			if (description != null)
			{
				try
				{
					embed.WithDescription(description);
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
		/// <summary>
		/// Adds an author to the embed. Verifies Urls exist and cuts things to appropriate length.
		/// </summary>
		/// <param name="embed"></param>
		/// <param name="name"></param>
		/// <param name="iconUrl"></param>
		/// <param name="url"></param>
		/// <returns></returns>
		public static EmbedBuilder MyAddAuthor(this EmbedBuilder embed, string name = null, string iconUrl = null, string url = null)
		{
			iconUrl = GetActions.GetIfStringIsValidUrl(iconUrl) ? iconUrl : null;
			url = GetActions.GetIfStringIsValidUrl(url) ? url : null;

			return embed.WithAuthor(x =>
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
		}
		/// <summary>
		/// Does the same thing as <see cref="MyAddAuthor(EmbedBuilder, string, string, string)"/> except uses username and avatar Url.
		/// </summary>
		/// <param name="embed"></param>
		/// <param name="user"></param>
		/// <param name="URL"></param>
		/// <returns></returns>
		public static EmbedBuilder MyAddAuthor(this EmbedBuilder embed, IUser user, string URL = null)
		{
			return embed.MyAddAuthor(user.Username, user.GetAvatarUrl(), URL ?? user.GetAvatarUrl());
		}
		/// <summary>
		/// Adds a footer to the embed. Verifies the Url exists and cuts the text to the appropriate length.
		/// </summary>
		/// <param name="embed"></param>
		/// <param name="text"></param>
		/// <param name="iconUrl"></param>
		/// <returns></returns>
		public static EmbedBuilder MyAddFooter(this EmbedBuilder embed, [CallerMemberName] string text = null, string iconUrl = null)
		{
			iconUrl = GetActions.GetIfStringIsValidUrl(iconUrl) ? iconUrl : null;

			return embed.WithFooter(x =>
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
		}
		/// <summary>
		/// Adds a field to the embed. Cuts the name and value to the appropriate length.
		/// </summary>
		/// <param name="embed"></param>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <param name="isInline"></param>
		/// <param name="prefix"></param>
		/// <returns></returns>
		public static EmbedBuilder MyAddField(this EmbedBuilder embed, string name, string value, bool isInline = true)
		{
			if (embed.Build().Fields.Count() >= Constants.MAX_FIELDS)
			{
				return embed;
			}

			name = String.IsNullOrWhiteSpace(name) ? "Placeholder" : name.Substring(0, Math.Min(Constants.MAX_FIELD_NAME_LENGTH, name.Length));
			value = String.IsNullOrWhiteSpace(name) ? "Placeholder" : value.Substring(0, Math.Min(Constants.MAX_FIELD_VALUE_LENGTH, value.Length));

			return embed.AddField(x =>
			{
				x.Name = name;
				x.Value = value;
				x.IsInline = isInline;
			});
		}
	}
}