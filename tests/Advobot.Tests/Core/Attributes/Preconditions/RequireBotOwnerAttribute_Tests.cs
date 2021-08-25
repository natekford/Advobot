
using Advobot.Attributes.Preconditions;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.Preconditions
{
	[TestClass]
	public sealed class RequireBotOwnerAttribute_Tests : PreconditionTestsBase
	{
		protected override PreconditionAttribute Instance { get; }
			= new RequireBotOwnerAttribute();

		[TestMethod]
		public async Task InvokerIsNotOwner_Test()
		{
			var result = await CheckPermissionsAsync().CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task InvokerIsOwner_Test()
		{
			Context.Client.FakeApplication.Owner = Context.User;

			var result = await CheckPermissionsAsync().CAF();
			Assert.IsTrue(result.IsSuccess);
		}
	}
}