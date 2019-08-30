using System;
using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Tests.Fakes.Services.GuildSettings;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.UnitTests.Attributes.ParameterPreconditions.Numbers
{
	[TestClass]
	public sealed class SelfRoleGroupAttribute_Tests
		: ParameterPreconditionsTestsBase<SelfRoleGroupAttribute>
	{
		private readonly IGuildSettings _Settings;

		public SelfRoleGroupAttribute_Tests()
		{
			_Settings = new GuildSettings();

			Services = new ServiceCollection()
				.AddSingleton<IGuildSettingsFactory>(new FakeGuildSettingsFactory(_Settings))
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task GroupExisting_Test()
		{
			_Settings.SelfAssignableGroups.Add(new SelfAssignableRoles(1));

			var result = await CheckAsync(1).CAF();
			Assert.AreEqual(false, result.IsSuccess);
		}

		[TestMethod]
		public async Task GroupNotExisting_Test()
		{
			var result = await CheckAsync(1).CAF();
			Assert.AreEqual(true, result.IsSuccess);
		}

		[TestMethod]
		public async Task ThrowsOnNotInt_Test()
		{
			Task Task() => CheckAsync("not int");
			await Assert.ThrowsExceptionAsync<ArgumentException>(Task).CAF();
		}
	}
}