using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using AdvorangesUtils;
using Discord;

namespace Advobot.Classes
{
	/// <summary>
	/// Wrapper class for <see cref="EmbedBuilder"/>.
	/// Allows for preemptive error checking and error swallowing.
	/// </summary>
	public sealed class EmbedWrapper
	{
		/// <summary>
		/// The base color to use for an embed.
		/// </summary>
		public static Color Base { get; } = new Color(255, 100, 000);
		/// <summary>
		/// The color to use for users joining.
		/// </summary>
		public static Color Join { get; } = new Color(000, 255, 000);
		/// <summary>
		/// The color to use for users leaving.
		/// </summary>
		public static Color Leave { get; } = new Color(255, 000, 000);
		/// <summary>
		/// The color to use for users being modified.
		/// </summary>
		public static Color UserEdit { get; } = new Color(051, 051, 255);
		/// <summary>
		/// The color to use for attachments on a message.
		/// </summary>
		public static Color Attachment { get; } = new Color(000, 204, 204);
		/// <summary>
		/// The color to use for a message being edited.
		/// </summary>
		public static Color MessageEdit { get; } = new Color(000, 000, 255);
		/// <summary>
		/// The color to use for a message being deleted.
		/// </summary>
		public static Color MessageDelete { get; } = new Color(255, 051, 051);
		/// <summary>
		/// The maximum length in lines a description can be before it won't render on mobile.
		/// </summary>
		public const int MAX_DESCRIPTION_LINES = 20;
		/// <summary>
		/// The maximum length in lines a field can be before it won't render on mobile.
		/// </summary>
		public const int MAX_FIELD_LINES = 5;
		private const string LINE_BREAKS = "Line Breaks";

		/// <summary>
		/// The title of the embed. Will either eat errors or throw if the constructor was told to throw.
		/// </summary>
		public string? Title
		{
			get => _Builder.Title;
			set
			{
				if (TryAddTitle(value, out var errors)) { return; }
				if (_ThrowOnInvalid) { throw CreateException(errors); }

				_Builder.Title = ShortenString(errors, value);
			}
		}
		/// <summary>
		/// The description of the embed. Will either eat errors or throw if the constructor was told to throw.
		/// </summary>
		public string? Description
		{
			get => _Builder.Description;
			set
			{
				if (TryAddDescription(value, out var errors)) { return; }
				if (_ThrowOnInvalid) { throw CreateException(errors); }

				var shortenedOnLines = ShortenString(errors.Where(x => x.SubProperty == LINE_BREAKS), value, true);
				_Builder.Description = ShortenString(errors.Where(x => x.SubProperty == null), shortenedOnLines);
			}
		}
		/// <summary>
		/// The url of the embed. Will either eat errors or throw if the constructor was told to throw.
		/// </summary>
		public string? Url
		{
			get => _Builder.Url;
			set
			{
				if (TryAddUrl(value, out var errors)) { return; }
				if (_ThrowOnInvalid) { throw CreateException(errors); }

				_Builder.Url = value;
			}
		}
		/// <summary>
		/// The thumnail url of the embed. Will either eat errors or throw if the constructor was told to throw.
		/// </summary>
		public string? ThumbnailUrl
		{
			get => _Builder.ThumbnailUrl;
			set
			{
				if (TryAddThumbnailUrl(value, out var errors)) { return; }
				if (_ThrowOnInvalid) { throw CreateException(errors); }

				_Builder.ThumbnailUrl = value;
			}
		}
		/// <summary>
		/// The image url of the embed. Will either eat errors or throw if the constructor was told to throw.
		/// </summary>
		public string? ImageUrl
		{
			get => _Builder.ImageUrl;
			set
			{
				if (TryAddImageUrl(value, out var errors)) { return; }
				if (_ThrowOnInvalid) { throw CreateException(errors); }

				_Builder.ImageUrl = value;
			}
		}
		/// <summary>
		/// The color of the embed. Will either eat errors or throw if the constructor was told to throw.
		/// </summary>
		public Color? Color
		{
			get => _Builder.Color;
			set => _Builder.Color = value;
		}
		/// <summary>
		/// The timestamp of the embed. Will either eat errors or throw if the constructor was told to throw.
		/// </summary>
		public DateTimeOffset? Timestamp
		{
			get => _Builder.Timestamp;
			set => _Builder.Timestamp = value;
		}
		/// <summary>
		/// The author of the embed. Will either eat errors or throw if the constructor was told to throw.
		/// </summary>
		public EmbedAuthorBuilder Author
		{
			get => _Builder.Author;
			set
			{
				if (value == null) { _Builder.Author = null; return; }
				if (TryAddAuthor(value?.Name, value?.Url, value?.IconUrl, out var errors)) { return; }
				if (_ThrowOnInvalid) { throw CreateException(errors); }

				//No need to error check the Urls since they are going to always be valid
				//Since Discord.Net checks them in the builder or throws
				_Builder.Author = new EmbedAuthorBuilder
				{
					Name = ShortenString(errors, value?.Name),
					Url = value?.Url,
					IconUrl = value?.IconUrl
				};
			}
		}
		/// <summary>
		/// The footer of the embed. Will either eat errors or throw if the constructor was told to throw.
		/// </summary>
		public EmbedFooterBuilder Footer
		{
			get => _Builder.Footer;
			set
			{
				if (value == null) { _Builder.Footer = null; return; }
				if (TryAddFooter(value?.Text, value?.IconUrl, out var errors)) { return; }
				if (_ThrowOnInvalid) { throw CreateException(errors); }

				//No need to error check the Urls since they are going to always be valid
				//Since Discord.Net checks them in the builder or throws
				_Builder.Footer = new EmbedFooterBuilder
				{
					Text = ShortenString(errors, value?.Text),
					IconUrl = value?.IconUrl
				};
			}
		}
		/// <summary>
		/// The fields of the embed. Will either eat errors or throw if the constructor was told to throw.
		/// </summary>
		public List<EmbedFieldBuilder> Fields
		{
			get => _Builder.Fields;
			set
			{
				if (_ThrowOnInvalid && value.Count > EmbedBuilder.MaxFieldCount)
				{
					throw CreateException(new[] { EmbedError.MaxLength(nameof(Fields), nameof(Fields.Count), value.Count, EmbedBuilder.MaxFieldCount) });
				}

				//Have to clear and do it step by step instead of temp list
				//Because TryAddField adds to the builder if success and also checks against total embed length
				_Builder.Fields.Clear();
				for (var i = 0; i < Math.Min(value.Count, EmbedBuilder.MaxFieldCount); ++i)
				{
					var f = value[i];
					if (TryAddField(f?.Name, f?.Value?.ToString(), f?.IsInline ?? false, out var errors)) { continue; }
					if (_ThrowOnInvalid) { throw CreateException(errors); }

					var fName = ShortenString(errors.Where(x => x.SubProperty == nameof(EmbedFieldBuilder.Name)), f?.Name);
					var fValue = ShortenString(errors.Where(x => x.SubProperty == LINE_BREAKS), f?.Value?.ToString(), true);
					fValue = ShortenString(errors.Where(x => x.SubProperty == nameof(EmbedFieldBuilder.Value)), fValue);
					//If there's a total length error then don't even bother trying to put any more fields in
					//Because it's impossible to know what length the name and value should be made
					if (errors.SingleOrDefault(x => x.SubProperty == nameof(EmbedBuilder.MaxEmbedLength)).RemainingLength > -1)
					{
						break;
					}

					//No need to check again 
					_Builder.Fields.Add(new EmbedFieldBuilder
					{
						Name = fName,
						Value = fValue,
						IsInline = f?.IsInline ?? false,
					});
				}
			}
		}
		/// <summary>
		/// Any errors which have happened when building the embed.
		/// </summary>
		public ImmutableList<EmbedError> Errors => _Errors.ToImmutableList();
		/// <summary>
		/// The values which have failed to set.
		/// </summary>
		public ImmutableDictionary<string, string?> FailedValues => _FailedValues.ToImmutableDictionary();

		private readonly bool _ThrowOnInvalid;
		private readonly EmbedBuilder _Builder = new EmbedBuilder
		{
			Color = Base,
			Timestamp = DateTimeOffset.UtcNow
		};
		private readonly List<EmbedError> _Errors = new List<EmbedError>();
		private readonly Dictionary<string, string?> _FailedValues = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Creates an <see cref="EmbedWrapper"/> that can throw on exceptions if <paramref name="throwOnInvalid"/> is true.
		/// </summary>
		/// <param name="throwOnInvalid">Whether or not to throw exceptions. True throws them, false swallows them via error checking.</param>
		public EmbedWrapper(bool throwOnInvalid)
		{
			_ThrowOnInvalid = throwOnInvalid;
		}
		/// <summary>
		/// Creates an <see cref="EmbedWrapper"/> that won't throw exceptions.
		/// </summary>
		public EmbedWrapper() : this(false) { }

		/// <summary>
		/// Attempts to modify the title. Does nothing if fails.
		/// </summary>
		/// <param name="title"></param>
		/// <param name="errors"></param>
		/// <returns></returns>
		public bool TryAddTitle(string? title, out List<EmbedError> errors)
		{
			errors = new List<EmbedError>();
			if (title?.Length > EmbedBuilder.MaxTitleLength)
			{
				errors.Add(EmbedError.MaxLength(nameof(Title), null, title, EmbedBuilder.MaxTitleLength));
			}
			var remainingLen = GetRemainingLength(nameof(Title));
			if (title?.Length > remainingLen)
			{
				errors.Add(EmbedError.LengthRemaining(nameof(Title), null, title, remainingLen));
			}

			if (errors.Any())
			{
				_Errors.AddRange(errors);
				_FailedValues[nameof(Title)] = title;
				return false;
			}

			_Builder.Title = title;
			_FailedValues.Remove(nameof(Title));
			return true;
		}
		/// <summary>
		/// Attempts to modify the description. Does nothing if fails.
		/// </summary>
		/// <param name="description"></param>
		/// <param name="errors"></param>
		/// <returns></returns>
		public bool TryAddDescription(string? description, out List<EmbedError> errors)
		{
			errors = new List<EmbedError>();
			if (description?.Length > EmbedBuilder.MaxDescriptionLength)
			{
				errors.Add(EmbedError.MaxLength(nameof(Description), null, description, EmbedBuilder.MaxDescriptionLength));
			}
			if (description?.CountLineBreaks() > MAX_DESCRIPTION_LINES)
			{
				errors.Add(EmbedError.MaxLength(nameof(Description), LINE_BREAKS, description, MAX_DESCRIPTION_LINES));
			}
			var remainingLen = GetRemainingLength(nameof(Description));
			if (description?.Length > remainingLen)
			{
				errors.Add(EmbedError.LengthRemaining(nameof(Description), null, description, remainingLen));
			}

			if (errors.Any())
			{
				_Errors.AddRange(errors);
				_FailedValues[nameof(Description)] = description;
				return false;
			}

			_Builder.Description = description;
			_FailedValues.Remove(nameof(Description));
			return true;
		}
		/// <summary>
		/// Attempts to modify the url. Does nothing if fails.
		/// </summary>
		/// <param name="url"></param>
		/// <param name="errors"></param>
		/// <returns></returns>
		public bool TryAddUrl(string? url, out List<EmbedError> errors)
		{
			errors = new List<EmbedError>();
			if (url != null && !url.IsValidUrl())
			{
				errors.Add(EmbedError.Url(nameof(Url), null, url));

				_Errors.AddRange(errors);
				_FailedValues[nameof(Url)] = url;
				return false;
			}

			_Builder.Url = url;
			_FailedValues.Remove(nameof(Url));
			return true;
		}
		/// <summary>
		/// Attempts to modify the thumbnail url. Does nothing if fails.
		/// </summary>
		/// <param name="thumbnailUrl"></param>
		/// <param name="errors"></param>
		/// <returns></returns>
		public bool TryAddThumbnailUrl(string? thumbnailUrl, out List<EmbedError> errors)
		{
			errors = new List<EmbedError>();
			if (thumbnailUrl != null && !thumbnailUrl.IsValidUrl())
			{
				errors.Add(EmbedError.Url(nameof(ThumbnailUrl), null, thumbnailUrl));

				_Errors.AddRange(errors);
				_FailedValues[nameof(ThumbnailUrl)] = thumbnailUrl;
				return false;
			}

			_Builder.ThumbnailUrl = thumbnailUrl;
			_FailedValues.Remove(nameof(ThumbnailUrl));
			return true;
		}
		/// <summary>
		/// Attempts to modify the image url. Does nothing if fails.
		/// </summary>
		/// <param name="imageUrl"></param>
		/// <param name="errors"></param>
		/// <returns></returns>
		public bool TryAddImageUrl(string? imageUrl, out List<EmbedError> errors)
		{
			errors = new List<EmbedError>();
			if (imageUrl != null && !imageUrl.IsValidUrl())
			{
				errors.Add(EmbedError.Url(nameof(ImageUrl), null, imageUrl));

				_Errors.AddRange(errors);
				_FailedValues[nameof(ImageUrl)] = imageUrl;
				return false;
			}

			_Builder.ImageUrl = imageUrl;
			_FailedValues.Remove(nameof(ImageUrl));
			return true;
		}
		/// <summary>
		/// Attempts to modify the author. Does nothing if fails.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="url"></param>
		/// <param name="iconUrl"></param>
		/// <param name="errors"></param>
		/// <returns></returns>
		public bool TryAddAuthor(string? name, string? url, string? iconUrl, out List<EmbedError> errors)
		{
			errors = new List<EmbedError>();
			if (name?.Length > EmbedAuthorBuilder.MaxAuthorNameLength)
			{
				errors.Add(EmbedError.MaxLength(nameof(Author), nameof(EmbedAuthorBuilder.Name), name, EmbedAuthorBuilder.MaxAuthorNameLength));
			}
			var remainingLen = GetRemainingLength(nameof(Author));
			if (name?.Length > remainingLen)
			{
				errors.Add(EmbedError.LengthRemaining(nameof(Author), nameof(EmbedAuthorBuilder.Name), name, remainingLen));
			}
			if (url != null && !url.IsValidUrl())
			{
				errors.Add(EmbedError.Url(nameof(Author), nameof(EmbedAuthorBuilder.Url), url));
			}
			if (iconUrl != null && !iconUrl.IsValidUrl())
			{
				errors.Add(EmbedError.Url(nameof(Author), nameof(EmbedAuthorBuilder.IconUrl), iconUrl));
			}

			if (errors.Any())
			{
				_Errors.AddRange(errors);
				_FailedValues[nameof(Author)] = $"{name}\n{url}\n{iconUrl}";
				return false;
			}

			_Builder.Author = new EmbedAuthorBuilder
			{
				Name = name,
				Url = url,
				IconUrl = iconUrl
			};
			_FailedValues.Remove(nameof(Author));
			return true;
		}
		/// <summary>
		/// Attempts to modify the author using a user. Does nothing if fails.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="errors"></param>
		/// <returns></returns>
		public bool TryAddAuthor(IUser user, out List<EmbedError> errors)
			=> TryAddAuthor(user?.Username, user?.GetAvatarUrl(), user?.GetAvatarUrl(), out errors);
		/// <summary>
		/// Attempts to modify the footer. Does nothing if fails.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="iconUrl"></param>
		/// <param name="errors"></param>
		/// <returns></returns>
		public bool TryAddFooter(string? text, string? iconUrl, out List<EmbedError> errors)
		{
			errors = new List<EmbedError>();
			if (text?.Length > EmbedFooterBuilder.MaxFooterTextLength)
			{
				errors.Add(EmbedError.MaxLength(nameof(Footer), nameof(EmbedFooterBuilder.Text), text, EmbedFooterBuilder.MaxFooterTextLength));
			}
			var remainingLen = GetRemainingLength(nameof(Footer));
			if (text?.Length > remainingLen)
			{
				errors.Add(EmbedError.LengthRemaining(nameof(Footer), nameof(EmbedFooterBuilder.Text), text, remainingLen));
			}
			if (iconUrl != null && !iconUrl.IsValidUrl())
			{
				errors.Add(EmbedError.Url(nameof(Footer), nameof(iconUrl), iconUrl));
			}

			if (errors.Any())
			{
				_Errors.AddRange(errors);
				_FailedValues[nameof(Footer)] = $"{text}\n{iconUrl}";
				return false;
			}

			_Builder.Footer = new EmbedFooterBuilder
			{
				Text = text,
				IconUrl = iconUrl
			};
			_FailedValues.Remove(nameof(Footer));
			return true;
		}
		/// <summary>
		/// Attempts to add a field. Does nothing if fails.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <param name="inline"></param>
		/// <param name="errors"></param>
		/// <returns></returns>
		public bool TryAddField(string? name, string? value, bool inline, out List<EmbedError> errors)
		{
			errors = new List<EmbedError>();
			if (_Builder.Fields.Count >= EmbedBuilder.MaxFieldCount)
			{
				errors.Add(new EmbedError(nameof(Fields), null, null, $"Max fields is {EmbedBuilder.MaxFieldCount}."));
			}
			if (string.IsNullOrWhiteSpace(name))
			{
				errors.Add(new EmbedError(nameof(Fields), nameof(EmbedFieldBuilder.Name), name, $"Cannot be null or whitespace."));
			}
			if (string.IsNullOrWhiteSpace(value))
			{
				errors.Add(new EmbedError(nameof(Fields), nameof(EmbedFieldBuilder.Value), value, $"Cannot be null or whitespace."));
			}
			if (value?.CountLineBreaks() > MAX_DESCRIPTION_LINES)
			{
				errors.Add(EmbedError.MaxLength(nameof(Fields), LINE_BREAKS, value, MAX_FIELD_LINES));
			}
			if (name?.Length > EmbedFieldBuilder.MaxFieldNameLength)
			{
				errors.Add(EmbedError.MaxLength(nameof(Fields), nameof(EmbedFieldBuilder.Name), name, EmbedFieldBuilder.MaxFieldNameLength));
			}
			if (value?.Length > EmbedFieldBuilder.MaxFieldValueLength)
			{
				errors.Add(EmbedError.MaxLength(nameof(Fields), nameof(EmbedFieldBuilder.Value), value, EmbedFieldBuilder.MaxFieldValueLength));
			}
			var remainingLen = GetRemainingLength(null);
			if (name?.Length > remainingLen)
			{
				errors.Add(EmbedError.LengthRemaining(nameof(Fields), nameof(EmbedFieldBuilder.Name), name, remainingLen));
			}
			if (value?.Length > remainingLen)
			{
				errors.Add(EmbedError.LengthRemaining(nameof(Fields), nameof(EmbedFieldBuilder.Value), value, remainingLen));
			}
			if (name?.Length + value?.Length > remainingLen)
			{
				errors.Add(EmbedError.LengthRemaining(nameof(Fields), nameof(EmbedBuilder.MaxEmbedLength), name + "\n" + value, remainingLen));
			}

			if (errors.Any())
			{
				_Errors.AddRange(errors);
				_FailedValues[$"Field {_Builder.Fields.Count}"] = $"{name}\n{value}";
				return false;
			}

			_FailedValues.Remove($"Field {_Builder.Fields.Count}");
			_Builder.Fields.Add(new EmbedFieldBuilder
			{
				Name = name,
				Value = value,
				IsInline = inline
			});
			return true;
		}
		/// <summary>
		/// Attempts to remove a field. Does nothing if fails.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="field"></param>
		/// <param name="errors"></param>
		/// <returns></returns>
		public bool TryRemoveField(int index, out EmbedFieldBuilder? field, out List<EmbedError> errors)
		{
			errors = new List<EmbedError>();
			field = default;
			if (index < 0)
			{
				errors.Add(new EmbedError(nameof(Fields), nameof(index), index, $"Cannot be less than 0."));
			}
			if (_Builder.Fields.Count == 0)
			{
				errors.Add(new EmbedError(nameof(Fields), nameof(index), index, $"No fields to remove."));
			}
			if (_Builder.Fields.Count - 1 < index)
			{
				errors.Add(new EmbedError(nameof(Fields), nameof(index), index, $"Out of bounds."));
			}

			if (errors.Any())
			{
				_Errors.AddRange(errors);
				return false;
			}

			field = _Builder.Fields[index];
			_Builder.Fields.RemoveAt(index);
			return true;
		}
		/// <summary>
		/// Attempts to modify a field. Does nothing if fails.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <param name="inLine"></param>
		/// <param name="errors"></param>
		/// <returns></returns>
		public bool TryModifyField(int index, string name, string value, bool inLine, out List<EmbedError> errors)
		{
			if (!TryRemoveField(index, out var field, out errors))
			{
				return false;
			}
			//If the field fails to be removed then it has to be reinserted
			if (!TryAddField(name, value, inLine, out errors))
			{
				_Builder.Fields.Insert(index, field);
				return false;
			}

			//Newest field is in the list, but wrong position now
			var newField = _Builder.Fields.Last();
			_Builder.Fields.RemoveAt(_Builder.Fields.Count - 1);
			_Builder.Fields.Insert(index, newField);
			return true;
		}
		/// <summary>
		/// Builds and returns the embed.
		/// </summary>
		/// <returns></returns>
		public Embed Build()
			=> _Builder.Build();
		/// <summary>
		/// Returns the calculated length of an embed.
		/// Can ignore the length of title, author, description, or footer
		/// </summary>
		/// <returns></returns>
		private int GetRemainingLength(string? propertyToDisregard)
		{
			//Gotten from https://github.com/RogueException/Discord.Net/blob/7837c4862cab32ecc432b3c6794277d92d89647d/src/Discord.Net.Core/Entities/Messages/Embed.cs#L60
			var currentLength = _Builder.Title?.Length
				+ _Builder.Author?.Name?.Length
				+ _Builder.Description?.Length
				+ _Builder.Footer?.Text?.Length
				+ _Builder.Fields.Sum(f => f.Name.Length + f.Value.ToString().Length) ?? 0;
			currentLength -= propertyToDisregard switch
			{
				nameof(Title) => _Builder.Title?.Length ?? 0,
				nameof(Author) => _Builder.Author?.Name?.Length ?? 0,
				nameof(Description) => _Builder.Description?.Length ?? 0,
				nameof(Footer) => _Builder.Footer?.Text?.Length ?? 0,
				_ => 0,
			};
			return EmbedBuilder.MaxEmbedLength - currentLength;
		}
		/// <summary>
		/// Shortens a string to the smallest valid length gotten from <paramref name="errors"/>.
		/// </summary>
		/// <param name="errors"></param>
		/// <param name="value"></param>
		/// <param name="newLines"></param>
		/// <returns></returns>
		private string? ShortenString(IEnumerable<EmbedError> errors, string? value, bool newLines = false)
		{
			if (value == null)
			{
				return null;
			}

			var remaining = errors.Select(x => x.RemainingLength).Where(x => x > -1).DefaultIfEmpty(-1).Min();
			if (remaining < 0)
			{
				return value;
			}

			var shortened = "";
			if (newLines)
			{
				var lines = value.Split('\n', '\r');
				for (var i = 0; i < Math.Min(lines.Length, remaining); ++i)
				{
					shortened += lines[i] + "\n";
				}
			}
			else
			{
				shortened = value.Substring(remaining);
			}
			return shortened.Length == 0 ? null : shortened;
		}
		/// <summary>
		/// Creates an <see cref="ArgumentException"/> with the formatted errors as its text and the caller as its param name.
		/// </summary>
		/// <param name="errors"></param>
		/// <param name="caller"></param>
		/// <returns></returns>
		private ArgumentException CreateException(IEnumerable<EmbedError> errors, [CallerMemberName] string caller = "")
			=> new ArgumentException(string.Join("\n", errors), caller);
		/// <summary>
		/// Returns all the failed values.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> _FailedValues.Join("\n\n", x => $"{x.Key}:\n{x.Value}");
		/// <summary>
		/// Converts an <see cref="EmbedWrapper"/> to a <see cref="Embed"/>.
		/// </summary>
		/// <param name="wrapper"></param>
		public static implicit operator Embed(EmbedWrapper wrapper)
			=> wrapper.Build();
	}
}
