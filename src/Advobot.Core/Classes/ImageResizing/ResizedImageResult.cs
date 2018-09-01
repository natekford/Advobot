using System;
using System.IO;
using ImageMagick;

namespace Advobot.Classes.ImageResizing
{
	/// <summary>
	/// The result of the resizing.
	/// </summary>
	public sealed class ResizedImageResult : IDisposable
	{
		/// <summary>
		/// The resized image's data.
		/// </summary>
		public MemoryStream Stream { get; }
		/// <summary>
		/// The format of the image.
		/// </summary>
		public MagickFormat Format { get; }
		/// <summary>
		/// Any error gotten when resizing.
		/// </summary>
		public string Error { get; }
		/// <summary>
		/// Whether or not it was resized successfully.
		/// </summary>
		public bool IsSuccess { get; }

		internal ResizedImageResult(MemoryStream stream, MagickFormat format, string error)
		{
			stream?.Seek(0, SeekOrigin.Begin);
			Stream = stream;
			Format = format;
			Error = error;
			IsSuccess = error == null && stream?.Length > 0;
		}

		/// <summary>
		/// Disposes <see cref="Stream"/>.
		/// </summary>
		public void Dispose()
			=> Stream?.Dispose();
	}
}