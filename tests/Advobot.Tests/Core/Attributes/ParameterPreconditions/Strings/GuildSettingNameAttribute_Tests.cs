using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Resources;
using Advobot.Services.GuildSettings;
using Advobot.Tests.Fakes.Services.GuildSettings;
using Advobot.Tests.PreconditionTestsBases;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.Strings
{
	[TestClass]
	public sealed class GuildSettingNameAttribute_Tests
		: ParameterlessParameterPreconditions_TestsBase<GuildSettingNameAttribute>
	{
		public GuildSettingNameAttribute_Tests()
		{
			Services = new ServiceCollection()
				.AddSingleton<IGuildSettingsFactory>(new FakeGuildSettingsFactory(new GuildSettings()))
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task AllSettingNames_Test()
		{
			var settings = new[]
			{
				GuildSettingNames.WelcomeMessage,
				GuildSettingNames.GoodbyeMessage,
				GuildSettingNames.GuildCulture,
				GuildSettingNames.Prefix,
				//GuildSettingNames.ServerLog,
				//GuildSettingNames.ModLog,
				//GuildSettingNames.ImageLog,
				GuildSettingNames.MuteRole,
				GuildSettingNames.NonVerboseErrors,
				GuildSettingNames.SpamPrevention,
				GuildSettingNames.RaidPrevention,
				GuildSettingNames.PersistentRoles,
				GuildSettingNames.BotUsers,
				GuildSettingNames.SelfAssignableGroups,
				GuildSettingNames.Quotes,
				//GuildSettingNames.LogActions,
				GuildSettingNames.IgnoredCommandChannels,
				//GuildSettingNames.IgnoredLogChannels,
				//GuildSettingNames.IgnoredXpChannels,
				GuildSettingNames.ImageOnlyChannels,
				GuildSettingNames.BannedPhraseStrings,
				GuildSettingNames.BannedPhraseRegex,
				GuildSettingNames.BannedPhraseNames,
				GuildSettingNames.BannedPhrasePunishments,
				GuildSettingNames.Rules,
				GuildSettingNames.CommandSettings,
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