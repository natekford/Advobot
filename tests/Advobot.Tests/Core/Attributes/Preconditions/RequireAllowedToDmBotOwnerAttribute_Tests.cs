using System.Threading.Tasks;

using Advobot.Attributes.Preconditions;
using Advobot.Services.BotSettings;
using Advobot.Tests.Fakes.Services.BotSettings;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.Preconditions
{
	[TestClass]
	public sealed class RequireAllowedToDmBotOwnerAttribute_Tests : PreconditionTestsBase
	{
		private readonly FakeBotSettings _BotSettings = new();

		protected override PreconditionAttribute Instance { get; }
			= new RequireAllowedToDmBotOwnerAttribute();

		[TestMethod]
		public async Task CanDmBotOwner_Test()
		{
			var result = await CheckPermissionsAsync().CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		[TestMethod]
		public async Task CannotDmBotOwner_Test()
		{
			_BotSettings.UsersUnableToDmOwner.Add(Context.User.Id);

			var result = await CheckPermissionsAsync().CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		protected override void ModifyServices(IServiceCollection services)
		{
			services
				.AddSingleton<IBotSettings>(_BotSettings);
		}
	}
}