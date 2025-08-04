using AdvorangesUtils;

using Discord;

using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Advobot.Embeds;

/// <summary>
/// Wrapper class for <see cref="EmbedBuilder"/>.
/// Allows for preemptive error checking and error swallowing.
/// </summary>
public sealed class EmbedWrapper
{
	/// <summary>
	/// The maximum length in lines a description can be before it won't render on mobile.
	/// </summary>
	public const int MAX_DESCRIPTION_LINES = 20;
	/// <summary>
	/// The maximum length in lines a field can be before it won't render on mobile.
	/// </summary>
	public const int MAX_FIELD_LINES = 5;

	private readonly EmbedBuilder _Embed;
	private readonly List<EmbedException> _Errors = [];

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
			if (!TryAddAuthor(value.Name, value.Url, value.IconUrl, out var errors))
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
			if (!TryAddDescription(value, out var errors))
			{
				Throw(errors);
			}
		}
	}
	/// <summary>
	/// Any errors which have happened when building the embed.
	/// </summary>
	public IReadOnlyList<EmbedException> Errors
		=> [.. _Errors];
	/// <inheritdoc cref="EmbedBuilder.Fields"/>
	public List<EmbedFieldBuilder> Fields
	{
		get => _Embed.Fields;
		set
		{
			if (value is null)
			{
				throw new ArgumentNullException(nameof(Fields));
			}
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
				if (!TryAddField(f.Name, f.Value?.ToString(), f.IsInline, out var errors))
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
			if (!TryAddFooter(value.Text, value.IconUrl, out var errors))
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
			if (!TryAddImageUrl(value, out var errors))
			{
				Throw(errors);
			}
		}
	}
	/// <inheritdoc cref="EmbedBuilder.Length"/>
	public int Length => _Embed.Length;
	/// <inheritdoc cref="EmbedBuilder.ThumbnailUrl"/>
	public string? ThumbnailUrl
	{
		get => _Embed.ThumbnailUrl;
		set
		{
			if (!TryAddThumbnailUrl(value, out var errors))
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
			if (!TryAddTitle(value, out var errors))
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
			if (!TryAddUrl(value, out var errors))
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
		=> _Errors.Join(x => $"{x.PropertyPath}:\n{x.Value}", "\n\n");

	/// <summary>
	/// Attempts to modify the author. Does nothing if fails.
	/// </summary>
	/// <param name="name"></param>
	/// <param name="url"></param>
	/// <param name="iconUrl"></param>
	/// <param name="errors"></param>
	/// <returns></returns>
	public bool TryAddAuthor(
		string? name,
		string? url,
		string? iconUrl,
		out IReadOnlyList<EmbedException> errors)
	{
		return CreateValidator(() =>
		{
			_Embed.Author = new EmbedAuthorBuilder
			{
				Name = name,
				Url = url,
				IconUrl = iconUrl
			};
		})
		.Property<EmbedAuthorBuilder, string?>(x => x.Name, name)
			.Max(EmbedAuthorBuilder.MaxAuthorNameLength)
			.Remaining(GetRemainingLength(nameof(Author)))
		.Validator.Property<EmbedAuthorBuilder, string?>(x => x.Url, url)
			.ValidUrl()
		.Validator.Property<EmbedAuthorBuilder, string?>(x => x.IconUrl, iconUrl)
			.ValidUrl()
		.Validator.Finalize(out errors);
	}

	/// <summary>
	/// Attempts to modify the description. Does nothing if fails.
	/// </summary>
	/// <param name="description"></param>
	/// <param name="errors"></param>
	/// <returns></returns>
	public bool TryAddDescription(
		string? description,
		out IReadOnlyList<EmbedException> errors)
	{
		return CreateValidator(() => _Embed.Description = description)
		.Property<EmbedBuilder, string?>(x => x.Description, description)
			.Max(EmbedBuilder.MaxDescriptionLength)
			.Remaining(GetRemainingLength(nameof(Description)))
			.MaxLines(MAX_DESCRIPTION_LINES)
		.Validator.Finalize(out errors);
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
		out IReadOnlyList<EmbedException> errors)
	{
		var remaining = GetRemainingLength(null);
		return CreateValidator(() =>
		{
			_Embed.Fields.Add(new EmbedFieldBuilder
			{
				Name = name,
				Value = value,
				IsInline = inline
			});
		})
		.Property<EmbedBuilder, int>(_ => _Embed.Fields.Count, _Embed.Fields.Count + 1)
			.Max(EmbedBuilder.MaxFieldCount)
		.Validator.Property<EmbedFieldBuilder, string?>(x => x.Name, name)
			.NotEmpty()
			.Max(EmbedFieldBuilder.MaxFieldNameLength)
			.Remaining(remaining)
		.Validator.Property<EmbedFieldBuilder, string?>(x => (string)x.Value, value)
			.NotEmpty()
			.Max(EmbedFieldBuilder.MaxFieldValueLength)
			.Remaining(remaining)
			.MaxLines(MAX_FIELD_LINES)
		.Validator.Property<EmbedFieldBuilder, string?>(x => x.Name + (string)x.Value, name + value)
			.Remaining(remaining)
		.Validator.Finalize(out errors);
	}

	/// <summary>
	/// Attempts to modify the footer. Does nothing if fails.
	/// </summary>
	/// <param name="text"></param>
	/// <param name="iconUrl"></param>
	/// <param name="errors"></param>
	/// <returns></returns>
	public bool TryAddFooter(
		string? text,
		string? iconUrl,
		out IReadOnlyList<EmbedException> errors)
	{
		return CreateValidator(() =>
		{
			_Embed.Footer = new EmbedFooterBuilder
			{
				Text = text,
				IconUrl = iconUrl
			};
		})
		.Property<EmbedFooterBuilder, string?>(x => x.Text, text)
			.Max(EmbedFooterBuilder.MaxFooterTextLength)
			.Remaining(GetRemainingLength(nameof(Footer)))
		.Validator.Property<EmbedFooterBuilder, string?>(x => x.IconUrl, iconUrl)
			.ValidUrl()
		.Validator.Finalize(out errors);
	}

	/// <summary>
	/// Attempts to modify the image url. Does nothing if fails.
	/// </summary>
	/// <param name="imageUrl"></param>
	/// <param name="errors"></param>
	/// <returns></returns>
	public bool TryAddImageUrl(
		string? imageUrl,
		out IReadOnlyList<EmbedException> errors)
	{
		return CreateValidator(() => _Embed.ImageUrl = imageUrl)
		.Property<EmbedBuilder, string?>(x => x.ImageUrl, imageUrl)
			.ValidUrl()
		.Validator.Finalize(out errors);
	}

	/// <summary>
	/// Attempts to modify the thumbnail url. Does nothing if fails.
	/// </summary>
	/// <param name="thumbnailUrl"></param>
	/// <param name="errors"></param>
	/// <returns></returns>
	public bool TryAddThumbnailUrl(
		string? thumbnailUrl,
		out IReadOnlyList<EmbedException> errors)
	{
		return CreateValidator(() => _Embed.ThumbnailUrl = thumbnailUrl)
		.Property<EmbedBuilder, string?>(x => x.ThumbnailUrl, thumbnailUrl)
			.ValidUrl()
		.Validator.Finalize(out errors);
	}

	/// <summary>
	/// Attempts to modify the title. Does nothing if fails.
	/// </summary>
	/// <param name="title"></param>
	/// <param name="errors"></param>
	/// <returns></returns>
	public bool TryAddTitle(
		string? title,
		out IReadOnlyList<EmbedException> errors)
	{
		return CreateValidator(() => _Embed.Title = title)
		.Property<EmbedBuilder, string?>(x => x.Title, title)
			.Max(EmbedBuilder.MaxTitleLength)
			.Remaining(GetRemainingLength(nameof(Title)))
		.Validator.Finalize(out errors);
	}

	/// <summary>
	/// Attempts to modify the url. Does nothing if fails.
	/// </summary>
	/// <param name="url"></param>
	/// <param name="errors"></param>
	/// <returns></returns>
	public bool TryAddUrl(
		string? url,
		out IReadOnlyList<EmbedException> errors)
	{
		return CreateValidator(() => _Embed.Url = url)
		.Property<EmbedBuilder, string?>(x => x.Url, url)
			.ValidUrl()
		.Validator.Finalize(out errors);
	}

	private static void Throw(
		IReadOnlyList<EmbedException> errors,
		[CallerMemberName] string property = "")
	{
		var innerException = new AggregateException(errors);
		throw new ArgumentException($"Unable to set {property}.", innerException);
	}

	private EmbedValidator CreateValidator(Action setter)
			=> new(setter, _Errors);

	private int GetRemainingLength(string? propertyToDisregard)
	{
		// Gotten from https://github.com/RogueException/Discord.Net/blob/7837c4862cab32ecc432b3c6794277d92d89647d/src/Discord.Net.Core/Entities/Messages/Embed.cs#L60
		return EmbedBuilder.MaxEmbedLength - Length + propertyToDisregard switch
		{
			nameof(Title) => _Embed.Title?.Length ?? 0,
			nameof(Author) => _Embed.Author?.Name?.Length ?? 0,
			nameof(Description) => _Embed.Description?.Length ?? 0,
			nameof(Footer) => _Embed.Footer?.Text?.Length ?? 0,
			nameof(Fields) => _Embed.Fields.Sum(f => f.Name.Length + f.Value.ToString().Length),
			_ => 0,
		};
	}
}