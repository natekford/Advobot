﻿using System.Threading.Tasks;

using Advobot.Attributes.Preconditions.QuantityLimits;
using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Tests.Fakes.Services.BotSettings;
using Advobot.Tests.Fakes.Services.GuildSettings;
using Advobot.Tests.PreconditionTestsBases;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.Preconditions.QuantityLimits
{
	[TestClass]
	public sealed class BannedStringsLimitAttribute_Tests
		: Preconditions_TestBase<BannedStringsLimitAttribute>
	{
		private readonly IBotSettings _BotSettings;
		private readonly IGuildSettings _Settings;
		private QuantityLimitAction _Action;

		protected override BannedStringsLimitAttribute Instance
			=> new BannedStringsLimitAttribute(_Action);

		public BannedStringsLimitAttribute_Tests()
		{
			_Settings = new GuildSettings();
			_BotSettings = new FakeBotSettings();

			Services = new ServiceCollection()
				.AddSingleton<IGuildSettingsFactory>(new FakeGuildSettingsFactory(_Settings))
				.AddSingleton(_BotSettings)
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task InvalidAdd_Test()
		{
			_Action = QuantityLimitAction.Add;
			_BotSettings.MaxBannedStrings = 0;

			var result = await CheckAsync().CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task InvalidRemove_Test()
		{
			_Action = QuantityLimitAction.Remove;
			_BotSettings.MaxBannedStrings = 1;

			var result = await CheckAsync().CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task ValidAdd_Test()
		{
			_Action = QuantityLimitAction.Add;
			_BotSettings.MaxBannedStrings = 1;

			var result = await CheckAsync().CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		[TestMethod]
		public async Task ValidRemove_Test()
		{
			_Action = QuantityLimitAction.Remove;
			_BotSettings.MaxBannedStrings = 1;
			_Settings.BannedPhraseStrings.Add(new BannedPhrase("test", PunishmentType.Nothing));

			var result = await CheckAsync().CAF();
			Assert.IsTrue(result.IsSuccess);
		}
	}
}