using Advobot.Quotes.Database;
using Advobot.Quotes.Models;
using Advobot.Quotes.ParameterPreconditions;
using Advobot.Tests.TestBases;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.Commands.Quotes.ParameterPreconditions;

[TestClass]
public sealed class QuoteName_Tests : ParameterPrecondition_Tests<QuoteName>
{
	private readonly FakeQuoteDatabase _Db = new();

	protected override QuoteName Instance { get; } = new();

	[TestMethod]
	public async Task QuoteExisting_Test()
	{
		var quote = new Quote
		{
			GuildId = Context.Guild.Id,
			Name = "dog",
			Description = "joe",
		};
		await _Db.AddQuoteAsync(quote).ConfigureAwait(false);

		await AssertFailureAsync(quote.Name).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task QuoteNotExisting_Test()
		=> await AssertSuccessAsync("i dont exist").ConfigureAwait(false);

	protected override void ModifyServices(IServiceCollection services)
	{
		services
			.AddSingleton<IQuoteDatabase>(_Db);
	}
}