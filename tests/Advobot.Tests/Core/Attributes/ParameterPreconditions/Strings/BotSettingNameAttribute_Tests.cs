using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Resources;
using Advobot.Services.BotSettings;
using Advobot.Tests.PreconditionTestsBases;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.Strings
{
	[TestClass]
	public sealed class BotSettingNameAttribute_Tests
		: ParameterlessParameterPreconditions_TestsBase<BotSettingNameAttribute>
	{
		public BotSettingNameAttribute_Tests()
		{
			Services = new ServiceCollection()
				.AddSingleton<IBotSettings>(new BotSettings())
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task AllSettingNames_Test()
		{
			var settings = new[]
			{
				BotSettingNames.LogLevel,
				BotSettingNames.Prefix,
				BotSettingNames.Game,
				BotSettingNames.Stream,
				BotSettingNames.AlwaysDownloadUsers,
				BotSettingNames.MessageCacheSize,
				BotSettingNames.MaxUserGatherCount,
				BotSettingNames.MaxMessageGatherSize,
				BotSettingNames.MaxRuleCategories,
				BotSettingNames.MaxRulesPerCategory,
				BotSettingNames.MaxSelfAssignableRoleGroups,
				BotSettingNames.MaxQuotes,
				BotSettingNames.MaxBannedRegex,
				BotSettingNames.MaxBannedNames,
				BotSettingNames.MaxBannedPunishments,
				BotSettingNames.TrustedUsers,
				BotSettingNames.UsersUnableToDmOwner,
				BotSettingNames.UsersIgnoredFromCommands,
			};
			foreach (var setting in settings)
			{
				var result = await CheckAsync(setting.ToLower()).CAF();
				Assert.IsTrue(result.IsSuccess);
			}
		}

		[TestMethod]
		public async Task FailsOnNotString_Test()
			=> await AssertPreconditionFailsOnInvalidType(CheckAsync(1)).CAF();

		[TestMethod]
		public async Task InvalidSetting_Test()
		{
			var result = await CheckAsync("not a setting").CAF();
			Assert.IsFalse(result.IsSuccess);
		}
	}
}