using Advobot.Modules;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using ImageMagick;

using System.Collections.Immutable;

namespace Advobot.Services.ImageResizing;

/// <summary>
/// Creates an emote on the specified guild.
/// </summary>
public sealed class EmoteCreationContext : ImageContextBase
{
	private static readonly ImmutableArray<MagickFormat> _AllValidFormats
		= _ValidStaticFormats.Concat(_ValidGifFormats).ToImmutableArray();

	private static readonly ImmutableArray<MagickFormat> _ValidGifFormats = new[]
	{
			MagickFormat.Mp4,
			MagickFormat.Gif,
		}.ToImmutableArray();

	private static readonly ImmutableArray<MagickFormat> _ValidStaticFormats = new[]
					{
			MagickFormat.Png,
			MagickFormat.Jpg,
			MagickFormat.Jpeg,
		}.ToImmutableArray();

	/// <inheritdoc />
	public override long MaxAllowedLengthInBytes => 256000;

	/// <summary>
	/// The name to give an emote.
	/// </summary>
	public string Name { get; }

	/// <inheritdoc />
	public override string Type => "Emote";

	/// <summary>
	/// Creates an instance of <see cref="EmoteCreationContext"/>.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="url"></param>
	/// <param name="args"></param>
	/// <param name="name"></param>
	public EmoteCreationContext(
		ICommandContext context,
		Uri url,
		UserProvidedImageArgs args,
		string name)
		: base(context, url, args)
	{
		Name = name;
	}

	/// <inheritdoc />
	public override IResult CanUseFormat(MagickFormat format)
	{
		var emoteCount = Context.Guild.PremiumTier switch
		{
			PremiumTier.Tier1 => 100,
			PremiumTier.Tier2 => 150,
			PremiumTier.Tier3 => 250,
			_ => 50,
		};

		var emotes = Context.Guild.Emotes;
		if (_ValidStaticFormats.Contains(format) && emotes.Count(x => !x.Animated) >= emoteCount)
		{
			return AdvobotResult.Failure($"There cannot be more than {emoteCount} non animated emotes.");
		}
		else if (_ValidGifFormats.Contains(format) && emotes.Count(x => x.Animated) >= emoteCount)
		{
			return AdvobotResult.Failure($"There cannot be more than {emoteCount} animated emotes.");
		}
		else if (!_AllValidFormats.Contains(format))
		{
			return AdvobotResult.Failure($"Cannot use an image with the format {format}.");
		}
		return AdvobotResult.IgnoreSuccess;
	}

	/// <inheritdoc />
	public override async Task<IResult> UseStream(MemoryStream stream)
	{
		try
		{
			await Context.Guild.CreateEmoteAsync(Name, new Image(stream), default, Context.GenerateRequestOptions()).CAF();
			return AdvobotResult.IgnoreSuccess;
		}
		catch (Exception e)
		{
			return AdvobotResult.Exception(e);
		}
	}
}