using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using ImageMagick;

namespace Advobot.Classes.ImageResizing
{
	/// <summary>
	/// Creates an emote on the specified guild.
	/// </summary>
	public sealed class EmoteCreationArgs : ImageArgs
	{
		private static ImmutableArray<MagickFormat> _ValidFormats { get; } = ImmutableArray.Create(new[]
		{
			MagickFormat.Png,
			MagickFormat.Jpg,
			MagickFormat.Jpeg,
			MagickFormat.Mp4,
			MagickFormat.Gif,
		});

		/// <inheritdoc />
		public override ImmutableArray<MagickFormat> ValidFormats => _ValidFormats;
		/// <inheritdoc />
		public override long MaxAllowedLengthInBytes => 256000;
		/// <inheritdoc />
		public override string Type => "Emote";
		/// <summary>
		/// The name to give an emote.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Creates an instance of <see cref="EmoteCreationArgs"/>.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="url"></param>
		/// <param name="userArgs"></param>
		/// <param name="name"></param>
		public EmoteCreationArgs(SocketCommandContext context, Uri url, UserProvidedImageArgs userArgs, string name)
			: base(context, url, userArgs)
		{
			Name = name;
		}

		/// <inheritdoc />
		public override async Task<IResult> UseStream(MemoryStream stream, MagickFormat? format)
		{
			try
			{
				await Context.Guild.CreateEmoteAsync(Name, new Image(stream), default, Context.GenerateRequestOptions()).CAF();
				return DefaultResult;
			}
			catch (Exception e)
			{
				return ImageResult.FromError(CommandError.Exception, e.Message);
			}
		}
		/// <inheritdoc />
		public override IResult CanUseImage()
		{
			return Context.Guild.Emotes.Count(x => !x.Animated) < 50
				? (IResult)DefaultResult
				: ImageResult.FromError(CommandError.UnmetPrecondition, "There are already 50 non animated emotes.");
		}
		/// <inheritdoc />
		public override IResult CanUseGif()
		{
			return Context.Guild.Emotes.Count(x => x.Animated) < 50
				? (IResult)DefaultResult
				: ImageResult.FromError(CommandError.UnmetPrecondition, "There are already 50 animated emotes.");
		}
	}
}
