using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Advobot.Core.Interfaces;
using Discord;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Wrapper class for <see cref="EmbedBuilder"/>.
	/// Allows for preemptive error checking and error swallowing.
	/// </summary>
	public class EmbedWrapper
	{
		//TODO: implement these checks
		public const int MAX_DESCRIPTION_LINES = 20;
		public const int MAX_FIELD_LINES = 5;

		private EmbedBuilder _Builder = new EmbedBuilder
		{
			Color = Constants.Base,
			Timestamp = DateTimeOffset.UtcNow
		};
		private bool _ThrowOnInvalid;

		private List<EmbedError> _Errors = new List<EmbedError>();
		public ImmutableList<EmbedError> Errors => _Errors.ToImmutableList();
		private Dictionary<string, string> _FailedValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		public ImmutableDictionary<string, string> FailedValues => _FailedValues.ToImmutableDictionary();

		public string Title
		{
			get => _Builder.Title;
			set
			{
				if (TryAddTitle(value, out var errors)) { return; }

				if (_ThrowOnInvalid) { throw CreateException(errors); }

				_Builder.Title = ShortenString(errors, value);
			}
		}
		public string Description
		{
			get => _Builder.Description;
			set
			{
				if (TryAddDescription(value, out var errors)) { return; }

				if (_ThrowOnInvalid) { throw CreateException(errors); }

				_Builder.Description = ShortenString(errors, value);
			}
		}
		public string Url
		{
			get => _Builder.Url;
			set
			{
				if (TryAddUrl(value, out var errors)) {
				}
				else if (_ThrowOnInvalid) { throw CreateException(errors); }
			}
		}
		public string ThumbnailUrl
		{
			get => _Builder.ThumbnailUrl;
			set
			{
				if (TryAddThumbnailUrl(value, out var errors)) {
				}
				else if (_ThrowOnInvalid) { throw CreateException(errors); }
			}
		}
		public string ImageUrl
		{
			get => _Builder.ImageUrl;
			set
			{
				if (TryAddImageUrl(value, out var errors)) {
				}
				else if (_ThrowOnInvalid) { throw CreateException(errors); }
			}
		}
		public Color? Color
		{
			get => _Builder.Color;
			set => _Builder.Color = value;
		}
		public DateTimeOffset? Timestamp
		{
			get => _Builder.Timestamp;
			set => _Builder.Timestamp = value;
		}
		public EmbedAuthorBuilder Author
		{
			get => _Builder.Author;
			set
			{
				if (value == null) { _Builder.Author = null; }
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
		public EmbedFooterBuilder Footer
		{
			get => _Builder.Footer;
			set
			{
				if (value == null) { _Builder.Footer = null; }
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
					var fValue = ShortenString(errors.Where(x => x.SubProperty == nameof(EmbedFieldBuilder.Value)), f?.Value?.ToString());
					//If there's a total length error then don't even bother trying to put any more fields in
					//Because it's impossible to know what length the name and value should be made
					if (errors.SingleOrDefault(x => x.SubProperty == nameof(EmbedBuilder.MaxEmbedLength)).RemainingLength > -1)
					{
						break;
					}

					//No need to check for 
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

		#region Attempts to modify, does nothing if fails
		public bool TryAddTitle(string title, out List<EmbedError> errors)
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

			if (!errors.Any())
			{
				_Builder.Title = title;
				_FailedValues.Remove(nameof(Title));
			}
			else
			{
				_Errors.AddRange(errors);
				_FailedValues[nameof(Title)] = title;
			}
			return !errors.Any();
		}
		public bool TryAddDescription(string description, out List<EmbedError> errors)
		{
			errors = new List<EmbedError>();
			if (description?.Length > EmbedBuilder.MaxDescriptionLength)
			{
				errors.Add(EmbedError.MaxLength(nameof(Description), null, description, EmbedBuilder.MaxDescriptionLength));
			}
			var remainingLen = GetRemainingLength(nameof(Description));
			if (description?.Length > remainingLen)
			{
				errors.Add(EmbedError.LengthRemaining(nameof(Description), null, description, remainingLen));
			}

			if (!errors.Any())
			{
				_Builder.Description = description;
				_FailedValues.Remove(nameof(Description));
			}
			else
			{
				_Errors.AddRange(errors);
				_FailedValues[nameof(Description)] = description;
			}
			return !errors.Any();
		}
		public bool TryAddUrl(string url, out List<EmbedError> errors)
		{
			errors = new List<EmbedError>();
			if (!IsValidUrl(url))
			{
				errors.Add(EmbedError.Url(nameof(Url), null, url));
			}

			if (!errors.Any())
			{
				_Builder.Url = url;
				_FailedValues.Remove(nameof(Url));
			}
			else
			{
				_Errors.AddRange(errors);
				_FailedValues[nameof(Url)] = url;
			}
			return !errors.Any();
		}
		public bool TryAddThumbnailUrl(string thumbnailUrl, out List<EmbedError> errors)
		{
			errors = new List<EmbedError>();
			if (!IsValidUrl(thumbnailUrl))
			{
				errors.Add(EmbedError.Url(nameof(ThumbnailUrl), null, thumbnailUrl));
			}

			if (!errors.Any())
			{
				_Builder.ThumbnailUrl = thumbnailUrl;
				_FailedValues.Remove(nameof(ThumbnailUrl));
			}
			else
			{
				_Errors.AddRange(errors);
				_FailedValues[nameof(ThumbnailUrl)] = thumbnailUrl;
			}
			return !errors.Any();
		}
		public bool TryAddImageUrl(string imageUrl, out List<EmbedError> errors)
		{
			errors = new List<EmbedError>();
			if (!IsValidUrl(imageUrl))
			{
				errors.Add(EmbedError.Url(nameof(ImageUrl), null, imageUrl));
			}

			if (!errors.Any())
			{
				_Builder.ImageUrl = imageUrl;
				_FailedValues.Remove(nameof(ImageUrl));
			}
			else
			{
				_Errors.AddRange(errors);
				_FailedValues[nameof(ImageUrl)] = imageUrl;
			}
			return !errors.Any();
		}
		public bool TryAddAuthor(string name, string url, string iconUrl, out List<EmbedError> errors)
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
			if (!IsValidUrl(url))
			{
				errors.Add(EmbedError.Url(nameof(Author), nameof(EmbedAuthorBuilder.Url), url));
			}
			if (!IsValidUrl(iconUrl))
			{
				errors.Add(EmbedError.Url(nameof(Author), nameof(EmbedAuthorBuilder.IconUrl), iconUrl));
			}

			if (!errors.Any())
			{
				_Builder.Author = new EmbedAuthorBuilder
				{
					Name = name,
					Url = url,
					IconUrl = iconUrl
				};
				_FailedValues.Remove(nameof(Author));
			}
			else
			{
				_Errors.AddRange(errors);
				_FailedValues[nameof(Author)] = $"{name}\n{url}\n{iconUrl}";
			}
			return !errors.Any();
		}
		public bool TryAddAuthor(IUser user, out List<EmbedError> errors)
		{
			return TryAddAuthor(user.Username, user.GetAvatarUrl(), user.GetAvatarUrl(), out errors);
		}
		public bool TryAddFooter(string text, string iconUrl, out List<EmbedError> errors)
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
			if (!IsValidUrl(iconUrl))
			{
				errors.Add(EmbedError.Url(nameof(Footer), nameof(iconUrl), iconUrl));
			}

			if (!errors.Any())
			{
				_Builder.Footer = new EmbedFooterBuilder
				{
					Text = text,
					IconUrl = iconUrl
				};
				_FailedValues.Remove(nameof(Footer));
			}
			else
			{
				_Errors.AddRange(errors);
				_FailedValues[nameof(Footer)] = $"{text}\n{iconUrl}";
			}
			return !errors.Any();
		}
		public bool TryAddField(string name, string value, bool inline, out List<EmbedError> errors)
		{
			errors = new List<EmbedError>();
			if (_Builder.Fields.Count >= EmbedBuilder.MaxFieldCount)
			{
				errors.Add(new EmbedError(nameof(Fields), null, null, $"Max fields is {EmbedBuilder.MaxFieldCount}."));
			}
			if (String.IsNullOrWhiteSpace(name))
			{
				errors.Add(new EmbedError(nameof(Fields), nameof(EmbedFieldBuilder.Name), name, $"Cannot be null or whitespace."));
			}
			if (String.IsNullOrWhiteSpace(value))
			{
				errors.Add(new EmbedError(nameof(Fields), nameof(EmbedFieldBuilder.Value), value, $"Cannot be null or whitespace."));
			}
			if (name.Length > EmbedFieldBuilder.MaxFieldNameLength)
			{
				errors.Add(EmbedError.MaxLength(nameof(Fields), nameof(EmbedFieldBuilder.Name), name, EmbedFieldBuilder.MaxFieldNameLength));
			}
			if (value.Length > EmbedFieldBuilder.MaxFieldValueLength)
			{
				errors.Add(EmbedError.MaxLength(nameof(Fields), nameof(EmbedFieldBuilder.Value), value, EmbedFieldBuilder.MaxFieldValueLength));
			}
			var remainingLen = GetRemainingLength(null);
			if (name.Length > remainingLen)
			{
				errors.Add(EmbedError.LengthRemaining(nameof(Fields), nameof(EmbedFieldBuilder.Name), name, remainingLen));
			}
			if (value.Length > remainingLen)
			{
				errors.Add(EmbedError.LengthRemaining(nameof(Fields), nameof(EmbedFieldBuilder.Value), value, remainingLen));
			}
			if (name.Length + value.Length > remainingLen)
			{
				errors.Add(EmbedError.LengthRemaining(nameof(Fields), nameof(EmbedBuilder.MaxEmbedLength), name + "\n" + value, remainingLen));
			}

			if (!errors.Any())
			{
				_FailedValues.Remove($"Field {_Builder.Fields.Count}");
				_Builder.Fields.Add(new EmbedFieldBuilder
				{
					Name = name,
					Value = value,
					IsInline = inline
				});
			}
			else
			{
				_Errors.AddRange(errors);
				_FailedValues[$"Field {_Builder.Fields.Count}"] = $"{name}\n{value}";
			}
			return !errors.Any();
		}
		public bool TryRemoveField(int index, out EmbedFieldBuilder field, out List<EmbedError> errors)
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

			if (!errors.Any())
			{
				field = _Builder.Fields[index];
				_Builder.Fields.RemoveAt(index);
			}
			_Errors.AddRange(errors);
			return !errors.Any();
		}
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
		#endregion

		/// <summary>
		/// Builds and returns the embed.
		/// </summary>
		/// <returns></returns>
		public Embed Build()
		{
			return _Builder.Build();
		}
		/// <summary>
		/// Returns true if the passed in string is a valid Url.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		private bool IsValidUrl(string input)
		{
			return String.IsNullOrEmpty(input) || (Uri.TryCreate(input, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp));
		}
		/// <summary>
		/// Returns the calculated length of an embed.
		/// Can ignore the length of title, author, description, or footer
		/// </summary>
		/// <returns></returns>
		private int GetRemainingLength(string propertyToDisregard)
		{
			//Gotten from https://github.com/RogueException/Discord.Net/blob/7837c4862cab32ecc432b3c6794277d92d89647d/src/Discord.Net.Core/Entities/Messages/Embed.cs#L60
			var currentLength = _Builder.Title?.Length
				+ _Builder.Author?.Name?.Length
				+ _Builder.Description?.Length
				+ _Builder.Footer?.Text?.Length
				+ _Builder.Fields.Sum(f => f.Name.Length + f.Value.ToString().Length) ?? 0;
			switch (propertyToDisregard)
			{
				case nameof(Title):
				{
					currentLength -= _Builder.Title?.Length ?? 0;
					break;
				}
				case nameof(Author):
				{
					currentLength -= _Builder.Author?.Name?.Length ?? 0;
					break;
				}
				case nameof(Description):
				{
					currentLength -= _Builder.Description?.Length ?? 0;
					break;
				}
				case nameof(Footer):
				{
					currentLength -= _Builder.Footer?.Text?.Length ?? 0;
					break;
				}
			}
			return EmbedBuilder.MaxEmbedLength - currentLength;
		}
		/// <summary>
		/// Shortens a string to the smallest valid length gotten from <paramref name="errors"/>.
		/// </summary>
		/// <param name="errors"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private string ShortenString(IEnumerable<EmbedError> errors, string value)
		{
			if (value == null)
			{
				return null;
			}

			var remaining = errors.Select(x => x.RemainingLength).Where(x => x > -1).DefaultIfEmpty(-1).Min();
			var shortened = value.Substring(0, remaining == -1 ? value.Length : remaining);
			return shortened.Length == 0 ? null : shortened;
		}
		/// <summary>
		/// Creates an <see cref="ArgumentException"/> with the formatted errors as its text and the caller as its param name.
		/// </summary>
		/// <param name="errors"></param>
		/// <param name="caller"></param>
		/// <returns></returns>
		private ArgumentException CreateException(IEnumerable<EmbedError> errors, [CallerMemberName] string caller = "")
		{
			return new ArgumentException(String.Join("\n", errors), caller);
		}

		public override string ToString()
		{
			return String.Join("\n\n", _FailedValues.Select(x => $"{x.Key}: {x.Value}"));
		}
	}

	/// <summary>
	/// Provides information about why something failed to add to an embed.
	/// </summary>
	public struct EmbedError : IError
	{
		public string Property { get; }
		public string SubProperty { get; }
		public string Value { get; }
		public string Reason { get; }
		public int RemainingLength { get; private set; }

		internal EmbedError(string property, string subProperty, object value, string reason)
		{
			Property = property;
			SubProperty = subProperty;
			Value = value.ToString();
			Reason = reason;
			RemainingLength = -1;
		}

		internal static EmbedError LengthRemaining(string property, string subProperty, object value, int lengthRemaining)
		{
			return new EmbedError(property, subProperty, value, $"Remaining length is {lengthRemaining}.")
			{
				RemainingLength = lengthRemaining
			};
		}
		internal static EmbedError MaxLength(string property, string subProperty, object value, int maxLength)
		{
			return new EmbedError(property, subProperty, value, $"Max length is {maxLength}.")
			{
				RemainingLength = maxLength
			};
		}
		internal static EmbedError Url(string property, string subProperty, object value)
		{
			return new EmbedError(property, subProperty, value, "Invalid url.");
		}

		public override string ToString()
		{
			var sb = new StringBuilder(Property);
			if (!String.IsNullOrWhiteSpace(SubProperty))
			{
				sb.Append("." + SubProperty);
			}
			sb.Append($": '{Value}' is invalid. Reason: {Reason}");
			return sb.ToString();
		}
	}
}
