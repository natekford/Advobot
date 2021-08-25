
using Advobot.Tests.TestBases;
using Advobot.TypeReaders;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders
{
	[TestClass]
	public sealed class InviteTypeReader_Tests : TypeReaderTestsBase
	{
		protected override TypeReader Instance { get; } = new InviteTypeReader();

		[TestMethod]
		public async Task Valid_Test()
		{
			var invite = await Context.Channel.CreateInviteAsync().CAF();

			var result = await ReadAsync(invite.Code).CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(IInviteMetadata));
		}
	}
}