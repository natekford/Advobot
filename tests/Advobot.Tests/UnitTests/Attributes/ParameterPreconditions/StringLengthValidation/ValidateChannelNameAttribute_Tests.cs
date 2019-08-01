using Advobot.Tests.Fakes.Services.GuildSettings;

namespace Advobot.Tests.UnitTests.Attributes.ParameterPreconditions.StringLengthValidation
{
	[TestClass]
	public sealed class ValidateChannelNameAttribute_Tests
		: ParameterPreconditionsTestsBase<ValidateChannelNameAttribute>
	{
		[TestMethod]
		public async Task ThrowsOnNotString_Test()
		{
			Task Task() => CheckAsync(1);
			await Assert.ThrowsExceptionAsync<ArgumentException>(Task).CAF();
		}
		[TestMethod]
		public async Task Standard_Test()
		{
			var expected = new Dictionary<string, bool>
			{
				{ new string('a', 1), false },
				{ new string('a', 2), true },
				{ new string('a', 100), true },
				{ new string('a', 101), false },
			};
			foreach (var kvp in expected)
			{
				var result = await CheckAsync(kvp.Key).CAF();
				Assert.AreEqual(kvp.Value, result.IsSuccess);
			}
		}
	}

	[TestClass]
	public sealed class ValidateRuleCategoryAttribute_Tests
		: ParameterPreconditionsTestsBase<ValidateRuleCategoryAttribute>
	{
		private readonly IGuildSettings _Settings;

		public ValidateRuleCategoryAttribute_Tests()
		{
			_Settings = new GuildSettings();

			Services = new ServiceCollection()
				.AddSingleton<IGuildSettingsFactory>(new FakeGuildSettingsFactory(_Settings))
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task ThrowsOnNotString_Test()
		{
			Task Task() => CheckAsync(1);
			await Assert.ThrowsExceptionAsync<ArgumentException>(Task).CAF();
		}
		[TestMethod]
		public async Task Standard_Test()
		{
			var expected = new Dictionary<string, bool>
			{
				{ "", false },
				{ new string('a', 1), true },
				{ new string('a', 250), true },
				{ new string('a', 251), false },
			};
			foreach (var kvp in expected)
			{
				var result = await CheckAsync(kvp.Key).CAF();
				Assert.AreEqual(kvp.Value, result.IsSuccess);
			}
		}
	}
}
