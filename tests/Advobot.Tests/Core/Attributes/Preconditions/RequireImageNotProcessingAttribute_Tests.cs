using System.Net.Http;
using System.Threading.Tasks;

using Advobot.Attributes.Preconditions;
using Advobot.Services.ImageResizing;
using Advobot.Tests.Fakes.Services.ImageResizing;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.Preconditions
{
	[TestClass]
	public sealed class RequireImageNotProcessingAttribute_Tests : PreconditionTestsBase
	{
		private readonly ImageResizer _Resizer = new ImageResizer(new HttpClient(new FakeImageResizingHttpMessageHandler()));

		protected override PreconditionAttribute Instance { get; }
			= new RequireImageNotProcessingAttribute();

		[TestMethod]
		public async Task IsNotProcessing_Test()
		{
			var result = await CheckPermissionsAsync().CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		[TestMethod]
		public async Task IsProcessing_Test()
		{
			_Resizer.Enqueue(new FakeImageContext(Context.Guild));

			var result = await CheckPermissionsAsync().CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		protected override void ModifyServices(IServiceCollection services)
		{
			services
				.AddSingleton<IImageResizer>(_Resizer);
		}
	}
}