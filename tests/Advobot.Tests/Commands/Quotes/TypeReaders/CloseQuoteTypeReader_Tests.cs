using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Quotes.Database;
using Advobot.Quotes.Models;
using Advobot.Quotes.ReadOnlyModels;
using Advobot.Tests.TestBases;
using Advobot.TypeReaders;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Quotes.TypeReaders
{
	[TestClass]
	public sealed class CloseQuoteTypeReader_Tests : TypeReaderTestsBase
	{
		private readonly FakeQuoteDatabase _Db = new();
		protected override TypeReader Instance { get; } = new CloseQuoteTypeReader();

		[TestMethod]
		public async Task Valid_Test()
		{
			var quote = new Quote
			{
				GuildId = Context.Guild.Id,
				Name = "dog",
				Description = "joe",
			};
			await _Db.AddQuoteAsync(quote).CAF();

			var result = await ReadAsync(quote.Name).CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(IEnumerable<IReadOnlyQuote>));
		}

		protected override void ModifyServices(IServiceCollection services)
		{
			services
				.AddSingleton<IQuoteDatabase>(_Db);
		}
	}
}