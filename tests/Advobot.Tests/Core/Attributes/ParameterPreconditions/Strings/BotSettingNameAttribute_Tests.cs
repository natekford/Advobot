using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Resources;
using Advobot.Services.BotSettings;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.Strings
{
	[TestClass]
	public sealed class BotSettingNameAttribute_Tests : ParameterPreconditionTestsBase
	{
		protected override ParameterPreconditionAttribute Instance { get; }
			= new BotSettingNameAttribute();

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
				BotSettingNames.UsersUnableToDmOwner,
				BotSettingNames.UsersIgnoredFromCommands,
			};
			foreach (var setting in settings)
			{
				var result = await CheckPermissionsAsync(setting.ToLower()).CAF();
				Assert.IsTrue(result.IsSuccess);
			}
		}

		[TestMethod]
		public async Task InvalidSetting_Test()
		{
			var result = await CheckPermissionsAsync("not a setting").CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		protected override void ModifyServices(IServiceCollection services)
		{
			services
				.AddSingleton<IBotSettings, BotSettings>();
		}
	}
}