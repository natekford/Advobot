using System.Net.Http;
using System.Threading.Tasks;

using Advobot.Attributes.Preconditions;
using Advobot.Services.ImageResizing;
using Advobot.Tests.Fakes.Services.ImageResizing;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.Preconditions
{
	[TestClass]
	public sealed class RequireImageNotProcessingAttribute_Tests
		: ParameterlessPreconditions_TestBase<RequireImageNotProcessingAttribute>
	{
		private readonly IImageResizer _Resizer;

		public RequireImageNotProcessingAttribute_Tests()
		{
			var handler = new FakeImageResizingHttpMessageHandler();
			var client = new HttpClient(handler);
			_Resizer = new ImageResizer(client);

			Services = new ServiceCollection()
				.AddSingleton(_Resizer)
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task IsNotProcessing_Test()
		{
			var result = await CheckAsync().CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		[TestMethod]
		public async Task IsProcessing_Test()
		{
			_Resizer.Enqueue(new FakeImageContext(Context.Guild));

			var result = await CheckAsync().CAF();
			Assert.IsFalse(result.IsSuccess);
		}
	}
}