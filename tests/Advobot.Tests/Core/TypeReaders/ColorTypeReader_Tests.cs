using System.Threading.Tasks;

using Advobot.TypeReaders;

using AdvorangesUtils;

using Discord;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders
{
	[TestClass]
	public sealed class ColorTypeReader_Tests
		: TypeReader_TestsBase<ColorTypeReader>
	{
		[TestMethod]
		public async Task Invalid_Test()
		{
			var result = await ReadAsync("asdf").CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task ValidEmpty_Test()
		{
			var result = await ReadAsync(null).CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(Color));
		}

		[TestMethod]
		public async Task ValidHex_Test()
		{
			var result = await ReadAsync(Color.Red.RawValue.ToString("X6")).CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(Color));
		}

		[TestMethod]
		public async Task ValidName_Test()
		{
			var result = await ReadAsync("Red").CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(Color));
		}

		[TestMethod]
		public async Task ValidRGB_Test()
		{
			var result = await ReadAsync("100/100/100").CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(Color));
		}
	}
}