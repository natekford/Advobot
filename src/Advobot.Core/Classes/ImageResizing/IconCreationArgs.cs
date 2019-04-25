using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using AdvorangesUtils;
using Discord.Commands;
using ImageMagick;

namespace Advobot.Classes.ImageResizing
{
	/// <summary>
	/// Creates an icon for the specified callback.
	/// </summary>
	public sealed class IconCreationArgs : ImageArgs
	{
		private static ImmutableArray<MagickFormat> _ValidFormats { get; } = ImmutableArray.Create(
			MagickFormat.Png,
			MagickFormat.Jpg,
			MagickFormat.Jpeg
		);

		/// <inheritdoc />
		public override ImmutableArray<MagickFormat> ValidFormats => _ValidFormats;
		/// <inheritdoc />
		public override long MaxAllowedLengthInBytes => 10000000;
		/// <inheritdoc />
		public override string Type { get; }

        private readonly Func<SocketCommandContext, MemoryStream, Task> _Callback;

		/// <summary>
		/// Creates an instance of <see cref="IconCreationArgs"/>.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="context"></param>
		/// <param name="url"></param>
		/// <param name="userArgs"></param>
		/// <param name="callback"></param>
		public IconCreationArgs(
			string type,
			SocketCommandContext context,
			Uri url,
			UserProvidedImageArgs? userArgs,
			Func<SocketCommandContext, MemoryStream, Task> callback)
			: base(context, url, userArgs)
		{
			Type = type;
			_Callback = callback;
		}

		/// <inheritdoc />
		public override async Task<IResult> UseStream(MemoryStream stream, MagickFormat? format)
		{
			try
			{
				await _Callback.Invoke(Context, stream).CAF();
				return DefaultResult;
			}
			catch (Exception e)
			{
				return ImageResult.FromError(CommandError.Exception, e.Message);
			}
		}
	}
}
