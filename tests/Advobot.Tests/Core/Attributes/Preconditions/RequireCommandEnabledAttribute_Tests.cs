using System;
using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Attributes.Preconditions;
using Advobot.Services.GuildSettings;
using Advobot.Services.HelpEntries;
using Advobot.Tests.Fakes.Services.GuildSettings;
using Advobot.Tests.Fakes.Services.HelpEntries;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.Preconditions
{
	[TestClass]
	public sealed class RequireCommandEnabledAttribute_Tests
		: ParameterlessPreconditions_TestBase<RequireCommandEnabledAttribute>
	{
		private const bool CAN_TOGGLE = true;
		private const string COMMAND_ID = "df7ceacd-7e49-42b0-a6d3-7a2453d731f1";
		private const bool IS_ENABLED = true;
		private readonly IModuleHelpEntry _HelpEntry;
		private readonly IGuildSettings _Settings;

		public RequireCommandEnabledAttribute_Tests()
		{
			_HelpEntry = new FakeHelpEntry
			{
				Id = COMMAND_ID,
				AbleToBeToggled = CAN_TOGGLE,
				EnabledByDefault = IS_ENABLED,
			};
			_Settings = new GuildSettings();

			Services = new ServiceCollection()
				.AddSingleton<IGuildSettingsFactory>(new FakeGuildSettingsFactory(_Settings))
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task Off_Test()
		{
			_Settings.CommandSettings.ModifyCommandValue(_HelpEntry, false);

			var result = await CheckAsync().CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task On_Test()
		{
			_Settings.CommandSettings.ModifyCommandValue(_HelpEntry, true);

			var result = await CheckAsync().CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		protected override async Task<CommandInfo?> GetCommandInfoAsync()
		{
			using var commands = new CommandService();
			var module = await commands.AddModuleAsync<FakeModule>(Services).CAF();
			return module.Commands[0];
		}

		[Meta(COMMAND_ID, CanToggle = CAN_TOGGLE, IsEnabled = IS_ENABLED)]
		[Group(nameof(FakeModule))]
		public sealed class FakeModule : ModuleBase
		{
			[Command]
			public Task EmptyCommandAsync() => throw new NotImplementedException();
		}
	}
}