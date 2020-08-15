using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Tests.Fakes.Services.GuildSettings;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.Strings
{
	[TestClass]
	public sealed class QuoteNameAttribute_Tests : ParameterPreconditionTestsBase
	{
		private readonly GuildSettings _Settings = new GuildSettings();

		protected override ParameterPreconditionAttribute Instance { get; }
			= new QuoteNameAttribute();

		[TestMethod]
		public async Task QuoteExisting_Test()
		{
			_Settings.Quotes.Add(new Quote { Name = "joe", Description = "bob" });

			var result = await CheckPermissionsAsync(_Settings.Quotes[0].Name).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task QuoteNotExisting_Test()
		{
			var result = await CheckPermissionsAsync("i dont exist").CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		protected override void ModifyServices(IServiceCollection services)
		{
			services
				.AddSingleton<IGuildSettingsFactory>(new FakeGuildSettingsFactory(_Settings));
		}
	}
}