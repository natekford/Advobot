using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Advobot.Attributes.Preconditions.QuantityLimits;
using Advobot.Services.GuildSettings;
using Advobot.Tests.Fakes.Services.GuildSettings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.Preconditions.QuantityLimits
{
	[TestClass]
	public sealed class BannedNamesLimitAttribute_Tests
		: Preconditions_TestBase<BannedNamesLimitAttribute>
	{
		private readonly IGuildSettings _Settings;
		private QuantityLimitAction _Action;

		public override BannedNamesLimitAttribute Instance
			=> new BannedNamesLimitAttribute(_Action);

		public BannedNamesLimitAttribute_Tests()
		{
			_Settings = new GuildSettings();

			Services = new ServiceCollection()
				.AddSingleton<IGuildSettingsFactory>(new FakeGuildSettingsFactory(_Settings))
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task InvalidAdd_Test()
		{
			_Action = QuantityLimitAction.Add;
		}

		[TestMethod]
		public async Task InvalidRemove_Test()
		{
			_Action = QuantityLimitAction.Remove;
		}

		[TestMethod]
		public async Task ValidAdd_Test()
		{
			_Action = QuantityLimitAction.Add;
		}

		[TestMethod]
		public async Task ValidRemove_Test()
		{
			_Action = QuantityLimitAction.Remove;
		}
	}
}