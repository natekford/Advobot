using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.BannedPhrases;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Tests.Fakes.Services.GuildSettings;
using Advobot.Tests.PreconditionTestsBases;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.BannedPhrases
{
	[TestClass]
	public sealed class NotAlreadyBannedRegexAttribute_Test
		: ParameterlessParameterPreconditions_TestsBase<NotAlreadyBannedRegexAttribute>
	{
		private readonly IGuildSettings _Settings;

		public NotAlreadyBannedRegexAttribute_Test()
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
		public async Task RegexExisting_Test()
		{
			const string PHRASE = "hi";

			_Settings.BannedPhraseRegex.Add(new BannedPhrase(PHRASE, PunishmentType.Nothing));

			var result = await CheckAsync(PHRASE).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task RegexNotExisting_Test()
		{
			var result = await CheckAsync("not existing").CAF();
			Assert.IsTrue(result.IsSuccess);
		}
	}
}