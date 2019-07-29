using System;
using System.IO;
using System.Threading.Tasks;
using Advobot.Interfaces;
using Discord.Commands;
using ImageMagick;

namespace Advobot.Services.ImageResizing
{
	/// <summary>
	/// Specifies how to use and resize an image.
	/// </summary>
	public interface IImageContext : IAsyncProgress<string>
	{
		/// <summary>
		/// The largest allowed file size.
		/// </summary>
		long MaxAllowedLengthInBytes { get; }
		/// <summary>
		/// What this is targeting, e.g. emote, profile picture, etc.
		/// </summary>
		string Type { get; }

		/// <summary>
		/// The guild this image context is for.
		/// </summary>
		ulong GuildId { get; }
		/// <summary>
		/// The url to download from.
		/// </summary>
		Uri Url { get; }
		/// <summary>
		/// The user provided arguments.
		/// </summary>
		UserProvidedImageArgs Args { get; }

		/// <summary>
		/// Uses the supplied stream to do the specified action.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		Task<IResult> UseStream(MemoryStream stream);
		/// <summary>
		/// Whether or not this context can use <paramref name="format"/>.
		/// </summary>
		/// <param name="format"></param>
		/// <returns></returns>
		IResult CanUseFormat(MagickFormat format);
		/// <summary>
		/// Sends the response to the context channel.
		/// </summary>
		/// <param name="result"></param>
		/// <returns></returns>
		Task SendFinalResponseAsync(IResult result);
	}
}