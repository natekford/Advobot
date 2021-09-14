using System.Collections.Immutable;

using Advobot.Modules;

using AdvorangesUtils;

using Discord.Commands;

using ImageMagick;

namespace Advobot.Services.ImageResizing
{
	/// <summary>
	/// Creates an icon for the specified callback.
	/// </summary>
	public sealed class IconCreationContext : ImageContextBase
	{
		private static readonly ImmutableArray<MagickFormat> _ValidFormats = ImmutableArray.Create(new[]
		{
			MagickFormat.Png,
			MagickFormat.Jpg,
			MagickFormat.Jpeg
		});

		private readonly Func<ICommandContext, MemoryStream, Task> _Callback;

		/// <inheritdoc />
		public override long MaxAllowedLengthInBytes => 10000000;

		/// <inheritdoc />
		public override string Type { get; }

		/// <summary>
		/// Creates an instance of <see cref="IconCreationContext"/>.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="url"></param>
		/// <param name="args"></param>
		/// <param name="type"></param>
		/// <param name="callback"></param>
		public IconCreationContext(
			ICommandContext context,
			Uri url,
			UserProvidedImageArgs? args,
			string type,
			Func<ICommandContext, MemoryStream, Task> callback)
			: base(context, url, args ?? new UserProvidedImageArgs())
		{
			Type = type;
			_Callback = callback;
		}

		/// <inheritdoc />
		public override IResult CanUseFormat(MagickFormat format)
		{
			if (_ValidFormats.Contains(format))
			{
				return AdvobotResult.IgnoreSuccess;
			}
			return AdvobotResult.Failure($"Cannot use an image with the format {format}.");
		}

		/// <inheritdoc />
		public override async Task<IResult> UseStream(MemoryStream stream)
		{
			try
			{
				await _Callback.Invoke(Context, stream).CAF();
				return AdvobotResult.IgnoreSuccess;
			}
			catch (Exception e)
			{
				return AdvobotResult.Exception(e);
			}
		}
	}
}