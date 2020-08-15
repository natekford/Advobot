using System.Threading.Tasks;

using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Tests.Fakes.Services.GuildSettings;
using Advobot.Tests.TestBases;
using Advobot.TypeReaders;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders
{
	[TestClass]
	public sealed class SelfAssignableRolesTypeReader_Tests : TypeReaderTestsBase
	{
		private readonly GuildSettings _Settings = new GuildSettings();
		protected override TypeReader Instance { get; } = new SelfAssignableRolesTypeReader();

		[TestMethod]
		public async Task Valid_Test()
		{
			var group = new SelfAssignableRoles(1);
			_Settings.SelfAssignableGroups.Add(group);

			var result = await ReadAsync(group.Group.ToString()).CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(SelfAssignableRoles));
		}

		protected override void ModifyServices(IServiceCollection services)
		{
			services
				.AddSingleton<IGuildSettingsFactory>(new FakeGuildSettingsFactory(_Settings));
		}
	}
}