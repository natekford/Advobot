
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.DiscordObjectValidation
{
	[TestClass]
	public sealed class NotBannedAttribute_Tests
		: ParameterPreconditionTestsBase<NotBannedAttribute>
	{
		private const ulong ID = 1;
		protected override NotBannedAttribute Instance { get; } = new();

		[TestMethod]
		public async Task BanExisting_Test()
		{
			await Context.Guild.AddBanAsync(ID).CAF();

			var result = await CheckPermissionsAsync(ID).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task BanNotExisting_Test()
		{
			var result = await CheckPermissionsAsync(ID).CAF();
			Assert.IsTrue(result.IsSuccess);
		}
	}
}