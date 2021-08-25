using Advobot.Quotes.Database;
using Advobot.Quotes.Models;
using Advobot.Quotes.ParameterPreconditions;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Quotes.ParameterPreconditions
{
	[TestClass]
	public sealed class QuoteNameAttribute_Tests
		: ParameterPreconditionTestsBase<QuoteNameAttribute>
	{
		private readonly FakeQuoteDatabase _Db = new();

		protected override QuoteNameAttribute Instance { get; } = new();

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

			await AssertFailureAsync(quote.Name).CAF();
		}

		[TestMethod]
		public async Task QuoteNotExisting_Test()
			=> await AssertSuccessAsync("i dont exist").CAF();

		protected override void ModifyServices(IServiceCollection services)
		{
			services
				.AddSingleton<IQuoteDatabase>(_Db);
		}
	}
}