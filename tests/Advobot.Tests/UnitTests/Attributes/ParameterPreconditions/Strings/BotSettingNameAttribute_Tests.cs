using System;
using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Resources;
using Advobot.Services.BotSettings;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.UnitTests.Attributes.ParameterPreconditions.Strings
{
	[TestClass]
	public sealed class BotSettingNameAttribute_Tests
		: ParameterPreconditionsTestsBase<BotSettingNameAttribute>
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
				Assert.AreEqual(true, result.IsSuccess);
			}
		}

		[TestMethod]
		public async Task InvalidSetting_Test()
		{
			var result = await CheckAsync("not a setting").CAF();
			Assert.AreEqual(false, result.IsSuccess);
		}

		[TestMethod]
		public async Task ThrowsOnNotString_Test()
		{
			Task Task() => CheckAsync(1);
			await Assert.ThrowsExceptionAsync<ArgumentException>(Task).CAF();
		}
	}
}