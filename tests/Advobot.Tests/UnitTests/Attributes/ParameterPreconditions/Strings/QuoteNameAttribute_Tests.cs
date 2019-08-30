using System;
using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Services.GuildSettings;
using Advobot.Tests.Fakes.Services.GuildSettings;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.UnitTests.Attributes.ParameterPreconditions.Strings
{
	[TestClass]
	public sealed class QuoteNameAttribute_Tests
		: ParameterPreconditionsTestsBase<QuoteNameAttribute>
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
		public async Task QuoteNotExisting_Test()
		{
			var result = await CheckAsync("i dont exist").CAF();
			Assert.AreEqual(true, result.IsSuccess);
		}

		[TestMethod]
		public async Task ThrowsOnNotString_Test()
		{
			Task Task() => CheckAsync(1);
			await Assert.ThrowsExceptionAsync<ArgumentException>(Task).CAF();
		}
	}
}