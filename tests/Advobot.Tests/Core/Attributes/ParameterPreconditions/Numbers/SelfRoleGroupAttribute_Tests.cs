using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Tests.Fakes.Services.GuildSettings;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.Numbers
{
	[TestClass]
	public sealed class SelfRoleGroupAttribute_Tests : ParameterPreconditionTestsBase
	{
		private readonly GuildSettings _Settings = new GuildSettings();

		protected override ParameterPreconditionAttribute Instance { get; }
			= new SelfRoleGroupAttribute();

		[TestMethod]
		public async Task GroupExisting_Test()
		{
			_Settings.SelfAssignableGroups.Add(new SelfAssignableRoles(1));

			var result = await CheckPermissionsAsync(1).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task GroupNotExisting_Test()
		{
			var result = await CheckPermissionsAsync(1).CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		protected override void ModifyServices(IServiceCollection services)
		{
			services
				.AddSingleton<IGuildSettingsFactory>(new FakeGuildSettingsFactory(_Settings));
		}
	}
}