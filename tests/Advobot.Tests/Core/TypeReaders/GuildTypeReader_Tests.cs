using System.Threading.Tasks;

using Advobot.Tests.TestBases;
using Advobot.TypeReaders;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders
{
	[TestClass]
	public sealed class GuildTypeReader_Tests : TypeReaderTestsBase
	{
		protected override TypeReader Instance { get; } = new GuildTypeReader();

		[TestMethod]
		public async Task ValidId_Test()
		{
			var result = await ReadAsync(Context.Guild.Id.ToString()).CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(IGuild));
		}

		[TestMethod]
		public async Task ValidName_Test()
		{
			var result = await ReadAsync(Context.Guild.Name).CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(IGuild));
		}
	}
}