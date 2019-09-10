using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Advobot.Tests.Fakes.Services.ImageResizing
{
	public sealed class FakeImageResizingHttpMessageHandler : HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken)
			=> throw new NotImplementedException();
	}
}