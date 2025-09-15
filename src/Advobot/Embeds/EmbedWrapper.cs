using Discord;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Advobot.Embeds;

/// <summary>
/// Wrapper class for <see cref="EmbedBuilder"/>.
/// Allows for preemptive error checking and error swallowing.
/// </summary>
public sealed class EmbedWrapper
{
	private readonly EmbedBuilder _Embed;

	/// <summary>
	/// The color to use for attachments on a message.
	/// </summary>
	public static Color Attachment { get; } = new Color(000, 204, 204);
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
	/// The color to use for a message being deleted.
	/// </summary>
	public static Color MessageDelete { get; } = new Color(255, 051, 051);
	/// <summary>
	/// The color to use for a message being edited.
	/// </summary>
	public static Color MessageEdit { get; } = new Color(000, 000, 255);
	/// <summary>
	/// The color to use for users being modified.
	/// </summary>
	public static Color UserEdit { get; } = new Color(051, 051, 255);

	/// <inheritdoc cref="EmbedBuilder.Author"/>
	public EmbedAuthorBuilder? Author
	{
		get => _Embed.Author;
		set
		{
			if (value is null)
			{
				_Embed.Author = null;
				return;
			}
			if (!TrySetAuthor(value.Name, value.Url, value.IconUrl, out var errors) && ThrowOnError)
			{
				Throw(errors);
			}
		}
	}
	/// <inheritdoc cref="EmbedBuilder.Color"/>
	public Color? Color
	{
		get => _Embed.Color;
		set => _Embed.Color = value;
	}
	/// <inheritdoc cref="EmbedBuilder.Description"/>
	public string? Description
	{
		get => _Embed.Description;
		set
		{
			if (!TrySetDescription(value, out var errors) && ThrowOnError)
			{
				Throw(errors);
			}
		}
	}
	/// <summary>
	/// Any errors which have happened when building the embed.
	/// </summary>
	public List<EmbedException> Errors => field ??= [];
	/// <inheritdoc cref="EmbedBuilder.Fields"/>
	public List<EmbedFieldBuilder> Fields
	{
		get => _Embed.Fields;
		set
		{
			ArgumentNullException.ThrowIfNull(value);
			if (value.Count > EmbedBuilder.MaxFieldCount)
			{
				throw new ArgumentException("Too many fields provided.");
			}

			// Have to clear and do it step by step instead of temp list
			// Because TryAddField adds to the builder if success and also checks against total embed length
			_Embed.Fields.Clear();
			for (var i = 0; i < Math.Min(value.Count, EmbedBuilder.MaxFieldCount); ++i)
			{
				var f = value[i];
				if (!TryAddField(f.Name, f.Value?.ToString(), f.IsInline, out var errors) && ThrowOnError)
				{
					Throw(errors, $"{nameof(Fields)}[{i}]");
				}
			}
		}
	}
	/// <inheritdoc cref="EmbedBuilder.Footer"/>
	public EmbedFooterBuilder? Footer
	{
		get => _Embed.Footer;
		set
		{
			if (value is null)
			{
				_Embed.Footer = null;
				return;
			}
			if (!TrySetFooter(value.Text, value.IconUrl, out var errors) && ThrowOnError)
			{
				Throw(errors);
			}
		}
	}
	/// <inheritdoc cref="EmbedBuilder.ImageUrl"/>
	public string? ImageUrl
	{
		get => _Embed.ImageUrl;
		set
		{
			if (!TrySetImageUrl(value, out var errors) && ThrowOnError)
			{
				Throw(errors);
			}
		}
	}
	/// <inheritdoc cref="EmbedBuilder.Length"/>
	public int Length => _Embed.Length;
	/// <summary>
	/// Whether or not to directly throw an exception when a property is given an invalid value.
	/// </summary>
	public bool ThrowOnError { get; set; } = true;
	/// <inheritdoc cref="EmbedBuilder.ThumbnailUrl"/>
	public string? ThumbnailUrl
	{
		get => _Embed.ThumbnailUrl;
		set
		{
			if (!TrySetThumbnailUrl(value, out var errors) && ThrowOnError)
			{
				Throw(errors);
			}
		}
	}
	/// <inheritdoc cref="EmbedBuilder.Timestamp"/>
	public DateTimeOffset? Timestamp
	{
		get => _Embed.Timestamp;
		set => _Embed.Timestamp = value;
	}
	/// <inheritdoc cref="EmbedBuilder.Title"/>
	public string? Title
	{
		get => _Embed.Title;
		set
		{
			if (!TrySetTitle(value, out var errors) && ThrowOnError)
			{
				Throw(errors);
			}
		}
	}
	/// <inheritdoc cref="EmbedBuilder.Url"/>
	public string? Url
	{
		get => _Embed.Url;
		set
		{
			if (!TrySetUrl(value, out var errors) && ThrowOnError)
			{
				Throw(errors);
			}
		}
	}

	/// <summary>
	/// Creates an instance of <see cref="EmbedWrapper"/>.
	/// </summary>
	public EmbedWrapper()
	{
		_Embed = new()
		{
			Color = Base,
			Timestamp = DateTimeOffset.UtcNow
		};
	}

	/// <summary>
	/// Creates an instance of <see cref="EmbedWrapper"/>.
	/// </summary>
	/// <param name="embed"></param>
	public EmbedWrapper(EmbedBuilder embed)
	{
		_Embed = embed;
	}

	/// <summary>
	/// Converts an <see cref="EmbedWrapper"/> to an <see cref="Embed"/>.
	/// </summary>
	/// <param name="wrapper"></param>
	public static implicit operator Embed(EmbedWrapper wrapper)
		=> wrapper.Build();

	/// <summary>
	/// Builds and returns the embed.
	/// </summary>
	/// <returns></returns>
	public Embed Build()
		=> _Embed.Build();

	/// <inheritdoc />
	public override string ToString()
	{
		var sb = new StringBuilder();
		foreach (var group in Errors.GroupBy(x => x.Path))
		{
			var sampleError = default(EmbedException?);
			foreach (var error in group)
			{
				if (sampleError is null)
				{
					sampleError = error;
					sb.Append("Path=").Append(sampleError.Path).AppendLine().Append("Reason=");
				}
				sb.Append(error.Message).Append(' ');
			}
			sb.AppendLine().Append("Value=").Append(sampleError!.Value).AppendLine().AppendLine();
		}
		return sb.ToString();
	}

	/// <summary>
	/// Attempts to add a field. Does nothing if fails.
	/// </summary>
	/// <param name="name"></param>
	/// <param name="value"></param>
	/// <param name="inline"></param>
	/// <param name="errors"></param>
	/// <returns></returns>
	public bool TryAddField(
		string? name,
		string? value,
		bool inline,
		[NotNullWhen(false)] out List<EmbedException>? errors)
	{
		errors = null;

		CheckMax(ref errors, "Fields", EmbedBuilder.MaxFieldCount, _Embed.Fields.Count + 1);
		CheckEmpty(ref errors, "Field.Name", name);
		CheckLength(ref errors, "Field.Name", EmbedFieldBuilder.MaxFieldNameLength, name);
		CheckEmpty(ref errors, "Field.Value", value);
		CheckLength(ref errors, "Field.Value", EmbedFieldBuilder.MaxFieldValueLength, value);
		CheckRemaining(ref errors, "Field.(Name+Value)", Ignore.None, name + value);

		if (errors is null)
		{
			_Embed.Fields.Add(new()
			{
				Name = name,
				Value = value,
				IsInline = inline
			});
		}
		return errors is null;
	}

	/// <summary>
	/// Attempts to modify the author. Does nothing if fails.
	/// </summary>
	/// <param name="name"></param>
	/// <param name="url"></param>
	/// <param name="iconUrl"></param>
	/// <param name="errors"></param>
	/// <returns></returns>
	public bool TrySetAuthor(
		string? name,
		string? url,
		string? iconUrl,
		[NotNullWhen(false)] out List<EmbedException>? errors)
	{
		errors = null;

		CheckLength(ref errors, "Author.Name", EmbedAuthorBuilder.MaxAuthorNameLength, name);
		CheckRemaining(ref errors, "Author.Name", Ignore.Author, name);
		CheckUrl(ref errors, "Author.Url", url);
		CheckUrl(ref errors, "Author.IconUrl", iconUrl);

		if (errors is null)
		{
			_Embed.Author = new()
			{
				Name = name,
				Url = url,
				IconUrl = iconUrl
			};
		}
		return errors is null;
	}

	/// <summary>
	/// Attempts to modify the description. Does nothing if fails.
	/// </summary>
	/// <param name="description"></param>
	/// <param name="errors"></param>
	/// <returns></returns>
	public bool TrySetDescription(
		string? description,
		[NotNullWhen(false)] out List<EmbedException>? errors)
	{
		errors = null;

		CheckLength(ref errors, "Description", EmbedBuilder.MaxDescriptionLength, description);
		CheckRemaining(ref errors, "Description", Ignore.Description, description);

		if (errors is null)
		{
			_Embed.Description = description;
		}
		return errors is null;
	}

	/// <summary>
	/// Attempts to modify the footer. Does nothing if fails.
	/// </summary>
	/// <param name="text"></param>
	/// <param name="iconUrl"></param>
	/// <param name="errors"></param>
	/// <returns></returns>
	public bool TrySetFooter(
		string? text,
		string? iconUrl,
		[NotNullWhen(false)] out List<EmbedException>? errors)
	{
		errors = null;

		CheckLength(ref errors, "Footer.Text", EmbedFooterBuilder.MaxFooterTextLength, text);
		CheckRemaining(ref errors, "Footer.Text", Ignore.Footer, text);
		CheckUrl(ref errors, "Footer.IconUrl", iconUrl);

		if (errors is null)
		{
			_Embed.Footer = new()
			{
				Text = text,
				IconUrl = iconUrl,
			};
		}
		return errors is null;
	}

	/// <summary>
	/// Attempts to modify the image url. Does nothing if fails.
	/// </summary>
	/// <param name="imageUrl"></param>
	/// <param name="errors"></param>
	/// <returns></returns>
	public bool TrySetImageUrl(
		string? imageUrl,
		[NotNullWhen(false)] out List<EmbedException>? errors)
	{
		errors = null;

		CheckUrl(ref errors, "ImageUrl", imageUrl);

		if (errors is null)
		{
			_Embed.ImageUrl = imageUrl;
		}
		return errors is null;
	}

	/// <summary>
	/// Attempts to modify the thumbnail url. Does nothing if fails.
	/// </summary>
	/// <param name="thumbnailUrl"></param>
	/// <param name="errors"></param>
	/// <returns></returns>
	public bool TrySetThumbnailUrl(
		string? thumbnailUrl,
		[NotNullWhen(false)] out List<EmbedException>? errors)
	{
		errors = null;

		CheckUrl(ref errors, "ThumbnailUrl", thumbnailUrl);

		if (errors is null)
		{
			_Embed.ThumbnailUrl = thumbnailUrl;
		}
		return errors is null;
	}

	/// <summary>
	/// Attempts to modify the title. Does nothing if fails.
	/// </summary>
	/// <param name="title"></param>
	/// <param name="errors"></param>
	/// <returns></returns>
	public bool TrySetTitle(
		string? title,
		[NotNullWhen(false)] out List<EmbedException>? errors)
	{
		errors = null;

		CheckLength(ref errors, "Title", EmbedBuilder.MaxTitleLength, title);
		CheckRemaining(ref errors, "Title", Ignore.Title, title);

		if (errors is null)
		{
			_Embed.Title = title;
		}
		return errors is null;
	}

	/// <summary>
	/// Attempts to modify the url. Does nothing if fails.
	/// </summary>
	/// <param name="url"></param>
	/// <param name="errors"></param>
	/// <returns></returns>
	public bool TrySetUrl(
		string? url,
		[NotNullWhen(false)] out List<EmbedException>? errors)
	{
		errors = null;

		CheckUrl(ref errors, "Url", url);

		if (errors is null)
		{
			_Embed.Url = url;
		}
		return errors is null;
	}

	private static void Throw(List<EmbedException> errors, [CallerMemberName] string property = "")
		=> throw new ArgumentException($"Unable to set {property}.", new AggregateException(errors));

	private void AddError(ref List<EmbedException>? errors, EmbedException error)
	{
		(errors ??= []).Add(error);
		Errors.Add(error);
	}

	private void CheckEmpty(ref List<EmbedException>? errors, string path, string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			AddError(ref errors, new("Cannot be null, empty, or whitespace.", path, value));
		}
	}

	private void CheckLength(ref List<EmbedException>? errors, string path, int max, string? value)
	{
		if (value is not null && value.Length > max)
		{
			AddError(ref errors, new($"Max length is {max}.", path, value));
		}
	}

	private void CheckMax(ref List<EmbedException>? errors, string path, int max, int value)
	{
		if (value > max)
		{
			AddError(ref errors, new($"Max count is {max}.", path, value));
		}
	}

	private void CheckRemaining(ref List<EmbedException>? errors, string path, Ignore property, string? value)
	{
		// Reference https://github.com/RogueException/Discord.Net/blob/7837c4862cab32ecc432b3c6794277d92d89647d/src/Discord.Net.Core/Entities/Messages/Embed.cs#L60
		var remaining = EmbedBuilder.MaxEmbedLength - Length + property switch
		{
			Ignore.Title => _Embed.Title?.Length ?? 0,
			Ignore.Author => _Embed.Author?.Name?.Length ?? 0,
			Ignore.Description => _Embed.Description?.Length ?? 0,
			Ignore.Footer => _Embed.Footer?.Text?.Length ?? 0,
			Ignore.Fields => _Embed.Fields.Sum(f => f.Name.Length + (f.Value?.ToString() ?? "").Length),
			_ => 0,
		};
		if (value is not null && value.Length > remaining)
		{
			AddError(ref errors, new($"Remaining length is {remaining}.", path, value));
		}
	}

	private void CheckUrl(ref List<EmbedException>? errors, string path, string? url)
	{
		if (url is not null && (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
			|| (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)))
		{
			AddError(ref errors, new("Invalid url.", path, url));
		}
	}

	private enum Ignore
	{
		None,
		Title,
		Author,
		Description,
		Footer,
		Fields,
	}
}