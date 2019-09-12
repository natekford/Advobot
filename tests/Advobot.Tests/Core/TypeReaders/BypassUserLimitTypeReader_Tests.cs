using System.Threading.Tasks;

using Advobot.TypeReaders;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders
{
	[TestClass]
	public sealed class BypassUserLimitTypeReader_Tests
		: TypeReader_TestsBase<BypassUserLimitTypeReader>
	{
		[TestMethod]
		public async Task Invalid_Test()
		{
			var result = await ReadAsync("asdf").CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(bool));
			Assert.IsFalse((bool)result.BestMatch);
		}

		[TestMethod]
		public async Task Valid_Test()
		{
			var result = await ReadAsync(BypassUserLimitTypeReader.BYPASS_STRING).CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(bool));
			Assert.IsTrue((bool)result.BestMatch);
		}
	}
}