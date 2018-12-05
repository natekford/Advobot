using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Advobot.Interfaces;
using Discord.Commands;
using ImageMagick;

namespace Advobot.Classes.ImageResizing
{
	/// <summary>
	/// How to use and resize an image.
	/// </summary>
	public abstract class ImageArgs : IImageArgs
	{
		/// <summary>
		/// The default success result to return.
		/// </summary>
		protected static IResult DefaultResult { get; } = new SuccessResult();

		/// <inheritdoc />
		public abstract ImmutableArray<MagickFormat> ValidFormats { get; }
		/// <inheritdoc />
		public abstract long MaxAllowedLengthInBytes { get; }
		/// <inheritdoc />
		public abstract string Type { get; }

		/// <inheritdoc />
		public SocketCommandContext Context { get; }
		/// <inheritdoc />
		public Uri Url { get; }
		/// <inheritdoc />
		public UserProvidedImageArgs UserArgs { get; }

		/// <summary>
		/// Creates an instance of <see cref="ImageArgs"/>.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="url"></param>
		/// <param name="userArgs"></param>
		public ImageArgs(SocketCommandContext context, Uri url, UserProvidedImageArgs userArgs)
		{
			Context = context ?? throw new ArgumentNullException(nameof(context));
			Url = url ?? throw new ArgumentNullException(nameof(url));
			UserArgs = userArgs ?? new UserProvidedImageArgs();
		}

		/// <inheritdoc />
		public abstract Task<IResult> UseStream(MemoryStream stream, MagickFormat format);
		/// <inheritdoc />
		public virtual IResult CanUseImage() => DefaultResult;
		/// <inheritdoc />
		public virtual IResult CanUseGif() => DefaultResult;

		/// <summary>
		/// A result which will only ever be success.
		/// </summary>
		private class SuccessResult : RuntimeResult
		{
			/// <summary>
			/// Creates an instance of <see cref="SuccessResult"/>.
			/// </summary>
			public SuccessResult() : base(null, null) { }
		}
	}
}
