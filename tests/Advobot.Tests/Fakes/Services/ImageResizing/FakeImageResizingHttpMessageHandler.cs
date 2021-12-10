namespace Advobot.Tests.Fakes.Services.ImageResizing;

public sealed class FakeImageResizingHttpMessageHandler : HttpMessageHandler
{
	protected override Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
		=> throw new NotImplementedException();
}