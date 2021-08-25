
using Advobot.Tests.TestBases;
using Advobot.TypeReaders;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders
{
	[TestClass]
	public sealed class UriTypeReader_Tests : TypeReaderTestsBase
	{
		protected override TypeReader Instance { get; } = new UriTypeReader();

		[TestMethod]
		public async Task Valid_Test()
		{
			var result = await ReadAsync("https://www.google.com").CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(Uri));
		}
	}
}