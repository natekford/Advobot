using Advobot.Modules;

using AdvorangesUtils;

using Discord.Commands;

using ImageMagick;

using System.Collections.Immutable;

namespace Advobot.Services.ImageResizing;

/// <summary>
/// Creates an icon for the specified callback.
/// </summary>
/// <remarks>
/// Creates an instance of <see cref="IconCreationContext"/>.
/// </remarks>
/// <param name="context"></param>
/// <param name="url"></param>
/// <param name="args"></param>
/// <param name="type"></param>
/// <param name="callback"></param>
public sealed class IconCreationContext(
	ICommandContext context,
	Uri url,
	UserProvidedImageArgs? args,
	string type,
	Func<ICommandContext, MemoryStream, Task> callback) : ImageContextBase(context, url, args ?? new UserProvidedImageArgs())
{
	private static readonly ImmutableArray<MagickFormat> _ValidFormats = ImmutableArray.Create(
	[
			MagickFormat.Png,
			MagickFormat.Jpg,
			MagickFormat.Jpeg
		]);

	private readonly Func<ICommandContext, MemoryStream, Task> _Callback = callback;

	/// <inheritdoc />
	public override long MaxAllowedLengthInBytes => 10000000;

	/// <inheritdoc />
	public override string Type { get; } = type;

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