using System.Threading.Tasks;

using Advobot.Quotes.Database;
using Advobot.Quotes.Models;
using Advobot.Quotes.ParameterPreconditions;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Quotes.ParameterPreconditions
{
	[TestClass]
	public sealed class QuoteNameAttribute_Tests : ParameterPreconditionTestsBase
	{
		private readonly FakeQuoteDatabase _Db = new();
		protected override ParameterPreconditionAttribute Instance { get; }
			= new QuoteNameAttribute();

		[TestMethod]
		public async Task QuoteExisting_Test()
		{
			var quote = new Quote
			{
				GuildId = Context.Guild.Id,
				Name = "dog",
				Description = "joe",
			};
			await _Db.AddQuoteAsync(quote).CAF();

			var result = await CheckPermissionsAsync(quote.Name).CAF();
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
				.AddSingleton<IQuoteDatabase>(_Db);
		}
	}
}