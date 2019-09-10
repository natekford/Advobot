using System;
using System.Threading.Tasks;

using Advobot.Attributes.Preconditions;
using Advobot.Services.BotSettings;
using Advobot.Tests.Fakes.Services.BotSettings;
using Advobot.Tests.PreconditionTestsBases;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.Preconditions
{
	[Obsolete]
	[TestClass]
	public sealed class RequireTrustedUserAttribute_Tests
		: ParameterlessPreconditions_TestBase<RequireTrustedUserAttribute>
	{
		private readonly IBotSettings _BotSettings;

		public RequireTrustedUserAttribute_Tests()
		{
			_BotSettings = new FakeBotSettings();

			Services = new ServiceCollection()
				.AddSingleton(_BotSettings)
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task IsNotTrustedUser_Test()
		{
			var result = await CheckAsync().CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task IsTrustedUser_Test()
		{
			_BotSettings.TrustedUsers.Add(Context.User.Id);

			var result = await CheckAsync().CAF();
			Assert.IsTrue(result.IsSuccess);
		}
	}
}