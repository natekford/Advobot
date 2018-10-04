using System;
using System.IO;
using Discord.Commands;
using ImageMagick;

namespace Advobot.Classes.ImageResizing
{
	/// <summary>
	/// The result of the resizing.
	/// </summary>
	public sealed class ImageResult : IResult, IDisposable
	{
		/// <summary>
		/// The resized image's data.
		/// </summary>
		public MemoryStream Stream { get; }
		/// <summary>
		/// The format of the image.
		/// </summary>
		public MagickFormat Format { get; }
		/// <inheritdoc />
		public CommandError? Error { get; }
		/// <inheritdoc />
		public string ErrorReason { get; }
		/// <inheritdoc />
		public bool IsSuccess => ErrorReason == null;

		private ImageResult(MemoryStream stream, MagickFormat format, CommandError? error, string errorReason)
		{
			stream?.Seek(0, SeekOrigin.Begin);
			Stream = stream;
			Format = format;
			Error = error;
			ErrorReason = errorReason;
		}

		/// <summary>
		/// Returns a result indicating success.
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="format"></param>
		/// <returns></returns>
		public static ImageResult FromSuccess(MemoryStream stream, MagickFormat format)
			=> new ImageResult(stream, format, null, null);
		/// <summary>
		/// Returns a result indicating failure.
		/// </summary>
		/// <param name="error"></param>
		/// <param name="errorReason"></param>
		/// <returns></returns>
		public static ImageResult FromError(CommandError error, string errorReason)
			=> new ImageResult(null, default, error, errorReason);
		/// <summary>
		/// Returns a result indicating failure.
		/// </summary>
		/// <param name="result"></param>
		/// <returns></returns>
		public static ImageResult FromError(IResult result)
			=> new ImageResult(null, default, result.Error, result.ErrorReason);

		/// <summary>
		/// Disposes <see cref="Stream"/>.
		/// </summary>
		public void Dispose()
			=> Stream?.Dispose();
	}
}