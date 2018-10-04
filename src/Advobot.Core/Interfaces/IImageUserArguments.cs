using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Advobot.Classes.ImageResizing;
using Discord.Commands;
using ImageMagick;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Specifies how to use and resize an image.
	/// </summary>
	public interface IImageArgs
	{
		/// <summary>
		/// Valid formats so an invalid image isn't attempted to resize.
		/// </summary>
		ImmutableArray<MagickFormat> ValidFormats { get; }
		/// <summary>
		/// The largest allowed file size.
		/// </summary>
		long MaxAllowedLengthInBytes { get; }
		/// <summary>
		/// What this is targeting, e.g. emote, profile picture, etc.
		/// </summary>
		string Type { get; }
		/// <summary>
		/// The context this was invoked in.
		/// </summary>
		SocketCommandContext Context { get; }
		/// <summary>
		/// The url to download from.
		/// </summary>
		Uri Url { get; }
		/// <summary>
		/// The user provided arguments.
		/// </summary>
		UserProvidedImageArgs UserArgs { get; }

		/// <summary>
		/// Uses the supplied stream to do the specified action.
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="format"></param>
		/// <returns></returns>
		Task<IResult> UseStream(MemoryStream stream, MagickFormat format);
		/// <summary>
		/// Determines whether an image can be used for the specified action.
		/// </summary>
		/// <returns></returns>
		IResult CanUseImage();
		/// <summary>
		/// Determines whether a gif can be used for the specified action.
		/// </summary>
		/// <returns></returns>
		IResult CanUseGif();
	}
}