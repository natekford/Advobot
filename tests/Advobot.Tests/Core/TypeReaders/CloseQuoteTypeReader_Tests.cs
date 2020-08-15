using System.Collections.Generic;
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
	public sealed class CloseQuoteTypeReader_Tests : TypeReaderTestsBase
	{
		private readonly GuildSettings _Settings = new GuildSettings();
		protected override TypeReader Instance { get; } = new CloseQuoteTypeReader();

		[TestMethod]
		public async Task Valid_Test()
		{
			_Settings.Quotes.Add(new Quote("dog", "kapow"));

			var result = await ReadAsync(_Settings.Quotes[0].Name).CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(IEnumerable<Quote>));
		}

		protected override void ModifyServices(IServiceCollection services)
		{
			services
				.AddSingleton<IGuildSettingsFactory>(new FakeGuildSettingsFactory(_Settings));
		}
	}
}