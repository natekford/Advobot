using Advobot.Core.Utilities;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Mostly functionally identical to <see cref="EmbedBuilder"/> except this implementation prioritizes this implementation's methods.
	/// </summary>
	public class EmbedWrapper : EmbedBuilder
	{
		private List<EmbedError> _Errors = new List<EmbedError>();
		public IReadOnlyList<EmbedError> Errors => _Errors.AsReadOnly();

		public EmbedWrapper(string title = null, string description = null, Color? color = null,
			string imageUrl = null, string url = null, string thumbnailUrl = null)
		{
			imageUrl = GetIfStringIsValidUrl(imageUrl) ? imageUrl : null;
			url = GetIfStringIsValidUrl(url) ? url : null;
			thumbnailUrl = GetIfStringIsValidUrl(thumbnailUrl) ? thumbnailUrl : null;

			WithColor(Constants.BASE);
			if (title != null)
			{
				WithTitle(title.Substring(0, Math.Min(Constants.MAX_TITLE_LENGTH, title.Length)));
			}
			if (description != null)
			{
				try
				{
					WithDescription(description);
				}
				catch (Exception e)
				{
					e.Write();
					_Errors.Add(new EmbedError("Description", description, e));
					WithDescription(e.Message);
				}
			}
			if (color != null)
			{
				WithColor(color.Value);
			}
			if (imageUrl != null)
			{
				WithImageUrl(imageUrl);
			}
			if (url != null)
			{
				WithUrl(url);
			}
			if (thumbnailUrl != null)
			{
				WithThumbnailUrl(thumbnailUrl);
			}
		}

		/// <summary>
		/// Adds an author to the embed. Verifies Urls exist and cuts things to appropriate length.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="iconUrl"></param>
		/// <param name="url"></param>
		/// <returns></returns>
		public EmbedWrapper AddAuthor(string name = null, string iconUrl = null, string url = null)
		{
			if (String.IsNullOrWhiteSpace(name) && String.IsNullOrWhiteSpace(iconUrl) && String.IsNullOrWhiteSpace(url))
			{
				return this;
			}

			iconUrl = GetIfStringIsValidUrl(iconUrl) ? iconUrl : null;
			url = GetIfStringIsValidUrl(url) ? url : null;

			WithAuthor(x =>
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
			return this;
		}
		/// <summary>
		/// Does the same thing as <see cref="AddAuthor(string, string, string)"/> except uses username and avatar Url.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="url"></param>
		/// <returns></returns>
		public EmbedWrapper AddAuthor(IUser user, string url = null)
			=> AddAuthor(user.Username, user.GetAvatarUrl(), url ?? user.GetAvatarUrl());
		/// <summary>
		/// Adds a footer to the embed. Verifies the Url exists and cuts the text to the appropriate length.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="iconUrl"></param>
		/// <returns></returns>
		public EmbedWrapper AddFooter([CallerMemberName] string text = null, string iconUrl = null)
		{
			if (String.IsNullOrWhiteSpace(text) && String.IsNullOrWhiteSpace(iconUrl))
			{
				return this;
			}

			iconUrl = GetIfStringIsValidUrl(iconUrl) ? iconUrl : null;

			WithFooter(x =>
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
			return this;
		}
		/// <summary>
		/// Adds a field to the embed. Cuts the name and value to the appropriate length.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <param name="isInline"></param>
		/// <returns></returns>
		public EmbedWrapper AddField(string name, string value, bool isInline = true)
		{
			if (String.IsNullOrWhiteSpace(name) || String.IsNullOrWhiteSpace(value))
			{
				return this;
			}
			else if (Fields.Count() >= Constants.MAX_FIELDS)
			{
				return this;
			}

			name = String.IsNullOrWhiteSpace(name) ? "Placeholder" : name.Substring(0, Math.Min(Constants.MAX_FIELD_NAME_LENGTH, name.Length));
			value = String.IsNullOrWhiteSpace(name) ? "Placeholder" : value.Substring(0, Math.Min(Constants.MAX_FIELD_VALUE_LENGTH, value.Length));

			AddField(x =>
			{
				x.Name = name;
				x.Value = value;
				x.IsInline = isInline;
			});
			return this;
		}

		/// <summary>
		/// Returns true if there are no issues with the description. Checks against various length limits.
		/// </summary>
		/// <param name="charCount"></param>
		/// <param name="badDescription"></param>
		/// <param name="error"></param>
		/// <returns></returns>
		public bool CheckIfValidDescription(int charCount, out string error)
		{
			if (charCount > Constants.MAX_EMBED_TOTAL_LENGTH - 1250)
			{
				error = $"`{Constants.MAX_EMBED_TOTAL_LENGTH}` char limit close.";
			}
			else if (Description?.Length > Constants.MAX_DESCRIPTION_LENGTH)
			{
				error = $"Over `{Constants.MAX_DESCRIPTION_LENGTH}` chars.";
			}
			else if (Description.CountLineBreaks() > Constants.MAX_DESCRIPTION_LINES)
			{
				error = $"Over `{Constants.MAX_DESCRIPTION_LINES}` lines.";
			}
			else
			{
				error = null;
			}

			return error == null;
		}
		/// <summary>
		/// Returns true if there are no issues with the field. Checks against various length limits.
		/// </summary>
		/// <param name="field"></param>
		/// <param name="charCount"></param>
		/// <param name="badValue"></param>
		/// <param name="error"></param>
		/// <returns></returns>
		public bool CheckIfValidField(EmbedFieldBuilder field, int charCount, out string error)
		{
			var value = field.Value.ToString();
			if (charCount > Constants.MAX_EMBED_TOTAL_LENGTH - 1500)
			{
				error = $"`{Constants.MAX_EMBED_TOTAL_LENGTH}` char limit close.";
			}
			else if (value?.Length > Constants.MAX_FIELD_VALUE_LENGTH)
			{
				error = $"Over `{Constants.MAX_FIELD_VALUE_LENGTH}` chars.";
			}
			else if (value.CountLineBreaks() > Constants.MAX_FIELD_LINES)
			{
				error = $"Over `{Constants.MAX_FIELD_LINES}` lines.";
			}
			else
			{
				error = null;
			}

			return error == null;
		}

		/// <summary>
		/// Returns true if the passed in string is a valid Url.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		private bool GetIfStringIsValidUrl(string input) => !String.IsNullOrWhiteSpace(input)
			&& Uri.TryCreate(input, UriKind.Absolute, out Uri uriResult)
			&& (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
	}
}
