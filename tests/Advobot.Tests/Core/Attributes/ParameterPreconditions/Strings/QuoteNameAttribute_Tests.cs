using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Services.GuildSettings;
using Advobot.Tests.Fakes.Services.GuildSettings;
using Advobot.Tests.PreconditionTestsBases;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.Strings
{
	[TestClass]
	public sealed class QuoteNameAttribute_Tests
		: ParameterlessParameterPreconditions_TestsBase<QuoteNameAttribute>
	{
		private readonly IGuildSettings _Settings;

		public QuoteNameAttribute_Tests()
		{
			_Settings = new GuildSettings();

			Services = new ServiceCollection()
				.AddSingleton<IGuildSettingsFactory>(new FakeGuildSettingsFactory(_Settings))
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task FailsOnNotString_Test()
			=> await AssertPreconditionFailsOnInvalidType(CheckAsync(1)).CAF();

		[TestMethod]
		public async Task QuoteNotExisting_Test()
		{
			var result = await CheckAsync("i dont exist").CAF();
			Assert.IsTrue(result.IsSuccess);
		}
	}
}