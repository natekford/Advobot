using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.DiscordObjectValidation
{
	[TestClass]
	public sealed class NotBannedAttribute_Tests
		: ParameterlessParameterPreconditions_TestsBase<NotBannedAttribute>
	{
		private const ulong BAN_ID = 1;

		[TestMethod]
		public async Task BanExisting_Test()
		{
			await Context.Guild.AddBanAsync(BAN_ID).CAF();

			var result = await CheckAsync(BAN_ID).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task BanNotExisting_Test()
		{
			var result = await CheckAsync(BAN_ID).CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		[TestMethod]
		public async Task FailsOnNotUlong_Test()
			=> await AssertPreconditionFailsOnInvalidType(CheckAsync("")).CAF();
	}
}