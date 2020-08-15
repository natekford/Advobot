using System;
using System.Reflection;
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
	public sealed class RequireCommandEnabledAttribute_Tests : PreconditionTestsBase
	{
		private readonly FakeHelpEntry _HelpEntry = new FakeHelpEntry
		{
			Id = typeof(FakeModule).GetCustomAttribute<MetaAttribute>()!.Guid.ToString(),
			AbleToBeToggled = typeof(FakeModule).GetCustomAttribute<MetaAttribute>()!.CanToggle,
			EnabledByDefault = typeof(FakeModule).GetCustomAttribute<MetaAttribute>()!.IsEnabled,
		};
		private readonly GuildSettings _Settings = new GuildSettings();

		protected override PreconditionAttribute Instance { get; }
			= new RequireCommandEnabledAttribute();

		[TestMethod]
		public async Task Off_Test()
		{
			_Settings.CommandSettings.ModifyCommandValue(_HelpEntry, false);

			var command = await GetCommandInfoAsync().CAF();
			var result = await CheckPermissionsAsync(command).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task On_Test()
		{
			_Settings.CommandSettings.ModifyCommandValue(_HelpEntry, true);

			var command = await GetCommandInfoAsync().CAF();
			var result = await CheckPermissionsAsync(command).CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		protected override void ModifyServices(IServiceCollection services)
		{
			services
				.AddSingleton<IGuildSettingsFactory>(new FakeGuildSettingsFactory(_Settings));
		}

		private async Task<CommandInfo?> GetCommandInfoAsync()
		{
			using var commands = new CommandService();
			var module = await commands.AddModuleAsync<FakeModule>(Services).CAF();
			return module.Commands[0];
		}

		[Meta("df7ceacd-7e49-42b0-a6d3-7a2453d731f1", CanToggle = true, IsEnabled = true)]
		[Group(nameof(FakeModule))]
		public sealed class FakeModule : ModuleBase
		{
			[Command]
			public Task EmptyCommandAsync() => throw new NotImplementedException();
		}
	}
}