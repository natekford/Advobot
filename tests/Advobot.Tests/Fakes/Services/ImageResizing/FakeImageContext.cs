using Advobot.Services.ImageResizing;

using Discord;
using Discord.Commands;

using ImageMagick;

namespace Advobot.Tests.Fakes.Services.ImageResizing;

public sealed class FakeImageContext(IGuild guild) : IImageContext
{
	public UserProvidedImageArgs Args => throw new NotImplementedException();
	public ulong GuildId { get; } = guild.Id;
	public long MaxAllowedLengthInBytes => throw new NotImplementedException();
	public string Type => throw new NotImplementedException();
	public Uri Url => throw new NotImplementedException();

	public IResult CanUseFormat(MagickFormat format)
		=> throw new NotImplementedException();

	public Task ReportAsync(string value)
		=> Task.CompletedTask;

	public Task SendFinalResponseAsync(IResult result)
		=> throw new NotImplementedException();

	public Task<IResult> UseStream(MemoryStream stream)
		=> throw new NotImplementedException();
}