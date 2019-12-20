using System.Threading.Tasks;

using Advobot.Classes;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Tests.Fakes.Services.GuildSettings;
using Advobot.TypeReaders;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders
{
	[TestClass]
	public sealed class SelfAssignableRoleTypeReader_Tests
		: TypeReader_TestsBase<SelfAssignableRoleTypeReader>
	{
		private readonly IGuildSettings _Settings;

		public SelfAssignableRoleTypeReader_Tests()
		{
			_Settings = new GuildSettings();

			Services = new ServiceCollection()
				.AddSingleton<IGuildSettingsFactory>(new FakeGuildSettingsFactory(_Settings))
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task Invalid_Test()
		{
			var result = await ReadAsync("asdf").CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task Valid_Test()
		{
			var role = await Context.Guild.CreateRoleAsync("asdf", null, null, false, null).CAF();
			var group = new SelfAssignableRoles(1);
			group.AddRoles(new[] { role });
			_Settings.SelfAssignableGroups.Add(group);

			var result = await ReadAsync(role.Name).CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(SelfAssignableRole));
		}
	}
}