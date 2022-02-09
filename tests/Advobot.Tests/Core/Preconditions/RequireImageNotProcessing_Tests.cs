using Advobot.Preconditions;
using Advobot.Services.ImageResizing;
using Advobot.Tests.Fakes.Services.ImageResizing;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Preconditions;

[TestClass]
public sealed class RequireImageNotProcessing_Tests
	: Precondition_Tests<RequireImageNotProcessing>
{
	private readonly ImageResizer _Resizer = new(new HttpClient(new FakeImageResizingHttpMessageHandler()));

	protected override RequireImageNotProcessing Instance { get; } = new();

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