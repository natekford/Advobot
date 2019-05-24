using System;
using System.Collections.Generic;
using Advobot.Classes.Formatting;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Classes
{
	/// <summary>
	/// Allows a user to make an embed.
	/// </summary>
	[NamedArgumentType]
	public sealed class CustomEmbed : IGuildFormattable
	{
		/// <summary>
		/// The title of the embed.
		/// </summary>
		public string? Title { get; set; }
		/// <summary>
		/// The description of the embed.
		/// </summary>
		public string? Description { get; set; }
		/// <summary>
		/// The image url of the embed.
		/// </summary>
		public Uri? ImageUrl { get; set; }
		/// <summary>
		/// The url of the embed.
		/// </summary>
		public Uri? Url { get; set; }
		/// <summary>
		/// The thumbnail url of the embed.
		/// </summary>
		public Uri? ThumbUrl { get; set; }
		/// <summary>
		/// The color of the embed.
		/// </summary>
		public Color? Color { get; set; }
		/// <summary>
		/// The author of the embed.
		/// </summary>
		public string? AuthorName { get; set; }
		/// <summary>
		/// The author's picture.
		/// </summary>
		public Uri? AuthorIconUrl { get; set; }
		/// <summary>
		/// The url to use when clicking on the author's name.
		/// </summary>
		public Uri? AuthorUrl { get; set; }
		/// <summary>
		/// The footer text.
		/// </summary>
		public string? Footer { get; set; }
		/// <summary>
		/// The footer's picture.
		/// </summary>
		public Uri? FooterIconUrl { get; set; }
		/// <summary>
		/// All of the fields on the embed.
		/// </summary>
		public IList<CustomField> FieldInfo { get; set; } = new List<CustomField>();

		/// <summary>
		/// Builds the embed from the fields.
		/// </summary>
		/// <returns></returns>
		public EmbedWrapper BuildWrapper()
		{
			var embed = new EmbedWrapper
			{
				Title = Title,
				Description = Description,
				Color = Color,
				ImageUrl = ImageUrl?.ToString(),
				Url = Url?.ToString(),
				ThumbnailUrl = ThumbUrl?.ToString(),
			};
			embed.TryAddAuthor(AuthorName, AuthorUrl?.ToString(), AuthorIconUrl?.ToString(), out _);
			embed.TryAddFooter(Footer, FooterIconUrl?.ToString(), out _);

			foreach (var field in FieldInfo)
			{
				embed.TryAddField(field.Name, field.Text, field.Inline, out _);
			}
			return embed;
		}
		/// <inheritdoc />
		public IDiscordFormattableString GetFormattableString()
		{
			var dict = new Dictionary<string, object?>
			{
				{ nameof(Title), Title },
				{ nameof(Description), Description },
				{ nameof(ImageUrl), ImageUrl },
				{ nameof(Url), Url },
				{ nameof(ThumbUrl), ThumbUrl },
				{ nameof(AuthorName), AuthorName },
				{ nameof(AuthorIconUrl), AuthorIconUrl },
				{ nameof(AuthorUrl), AuthorUrl },
				{ nameof(Footer), Footer },
				{ nameof(FooterIconUrl), FooterIconUrl },
			};
			dict.RemoveAll(x => x.Value == null);
			for (var i = 0; i < FieldInfo.Count; ++i)
			{
				var field = FieldInfo[i];
				if (field != null)
				{
					dict.Add($"Field {i}", field);
				}
			}
			return dict.ToDiscordFormattableStringCollection();
		}
	}
}
