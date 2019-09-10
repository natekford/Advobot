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
	[TestClass]
	public sealed class RequireAllowedToDmBotOwnerAttribute_Tests
		: ParameterlessPreconditions_TestBase<RequireAllowedToDmBotOwnerAttribute>
	{
		private readonly IBotSettings _BotSettings;

		public RequireAllowedToDmBotOwnerAttribute_Tests()
		{
			_BotSettings = new FakeBotSettings();

			Services = new ServiceCollection()
				.AddSingleton(_BotSettings)
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task CanDmBotOwner_Test()
		{
			var result = await CheckAsync().CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		[TestMethod]
		public async Task CannotDmBotOwner_Test()
		{
			_BotSettings.UsersUnableToDmOwner.Add(Context.User.Id);

			var result = await CheckAsync().CAF();
			Assert.IsFalse(result.IsSuccess);
		}
	}
}