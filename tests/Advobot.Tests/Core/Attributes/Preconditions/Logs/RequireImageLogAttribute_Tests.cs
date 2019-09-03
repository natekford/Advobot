using System.Threading.Tasks;

using Advobot.Attributes.Preconditions.Logs;
using Advobot.Services.GuildSettings;
using Advobot.Tests.Fakes.Services.GuildSettings;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.Preconditions.Logs
{
	[TestClass]
	public sealed class RequireImageLogAttribute_Tests
		: ParameterlessPreconditions_TestBase<RequireImageLogAttribute>
	{
		private readonly IGuildSettings _Settings;

		public RequireImageLogAttribute_Tests()
		{
			_Settings = new GuildSettings();

			Services = new ServiceCollection()
				.AddSingleton<IGuildSettingsFactory>(new FakeGuildSettingsFactory(_Settings))
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task DoesNotHaveLog_Test()
		{
			_Settings.ImageLogId = 0;

			var result = await CheckAsync().CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task HasLog_Test()
		{
			_Settings.ImageLogId = 73;

			var result = await CheckAsync().CAF();
			Assert.IsTrue(result.IsSuccess);
		}
	}
}