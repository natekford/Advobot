using Discord;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Advobot
{
	namespace Actions
	{
		public static class EmbedActions
		{
			//TODO: Figure out which method calls need to implement the prefix parameter
			public static EmbedBuilder MakeNewEmbed(string title = null, string description = null, Color? color = null, string imageURL = null, string URL = null, string thumbnailURL = null, string prefix = Constants.BOT_PREFIX)
			{
				//Make the embed builder
				var embed = new EmbedBuilder().WithColor(Constants.BASE);

				//Validate the URLs
				imageURL = UploadActions.ValidateURL(imageURL) ? imageURL : null;
				URL = UploadActions.ValidateURL(URL) ? URL : null;
				thumbnailURL = UploadActions.ValidateURL(thumbnailURL) ? thumbnailURL : null;

				//Add in the properties
				if (title != null)
				{
					embed.WithTitle(title.Substring(0, Math.Min(Constants.MAX_TITLE_LENGTH, title.Length)));
				}
				if (description != null)
				{
					embed.WithDescription(description.Replace(Constants.BOT_PREFIX, prefix));
				}
				if (color != null)
				{
					embed.WithColor(color.Value);
				}
				if (imageURL != null)
				{
					embed.WithImageUrl(imageURL);
				}
				if (URL != null)
				{
					embed.WithUrl(URL);
				}
				if (thumbnailURL != null)
				{
					embed.WithThumbnailUrl(thumbnailURL);
				}

				return embed;
			}
			public static void AddAuthor(EmbedBuilder embed, string name = null, string iconURL = null, string URL = null)
			{
				//Create the author builder
				var author = new EmbedAuthorBuilder();

				//Verify the URLs
				iconURL = UploadActions.ValidateURL(iconURL) ? iconURL : null;
				URL = UploadActions.ValidateURL(URL) ? URL : null;

				//Add in the properties
				if (name != null)
				{
					author.WithName(name.Substring(0, Math.Min(Constants.MAX_TITLE_LENGTH, name.Length)));
				}
				if (iconURL != null)
				{
					author.WithIconUrl(iconURL);
				}
				if (URL != null)
				{
					author.WithUrl(URL);
				}

				embed.WithAuthor(author);
			}
			public static void AddAuthor(EmbedBuilder embed, IUser user, string URL = null)
			{
				AddAuthor(embed, user.Username, user.GetAvatarUrl(), URL ?? user.GetAvatarUrl());
			}
			public static void AddFooter(EmbedBuilder embed, [CallerMemberName] string text = null, string iconURL = null)
			{
				//Make the footer builder
				var footer = new EmbedFooterBuilder();

				//Verify the URL
				iconURL = UploadActions.ValidateURL(iconURL) ? iconURL : null;

				//Add in the properties
				if (text != null)
				{
					footer.WithText(text.Substring(0, Math.Min(Constants.MAX_FOOTER_LENGTH, text.Length)));
				}
				if (iconURL != null)
				{
					footer.WithIconUrl(iconURL);
				}

				embed.WithFooter(footer);
			}
			public static void AddField(EmbedBuilder embed, string name, string value, bool isInline = true, string prefix = Constants.BOT_PREFIX)
			{
				if (embed.Build().Fields.Count() >= Constants.MAX_FIELDS)
					return;

				//Get the name and value
				name = String.IsNullOrWhiteSpace(name) ? "Placeholder" : name.Substring(0, Math.Min(Constants.MAX_FIELD_NAME_LENGTH, name.Length));
				value = String.IsNullOrWhiteSpace(name) ? "Placeholder" : value.Replace(Constants.BOT_PREFIX, prefix).Substring(0, Math.Min(Constants.MAX_FIELD_VALUE_LENGTH, value.Length));

				embed.AddField(x =>
				{
					x.Name = name;
					x.Value = value;
					x.IsInline = isInline;
				});
			}
		}
	}
}