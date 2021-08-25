using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.Numbers
{
	[TestClass]
	public sealed class NotBotOwnerAttribute_Tests
		: ParameterPreconditionTestsBase<NotBotOwnerAttribute>
	{
		protected override NotBotOwnerAttribute Instance { get; } = new();

		[TestMethod]
		public async Task Invalid_Test()
		{
			Context.Client.FakeApplication.Owner = Context.User;

			await AssertFailureAsync(Context.User.Id).CAF();
		}

		[TestMethod]
		public async Task Valid_Test()
			=> await AssertSuccessAsync(Context.User.Id).CAF();
	}
}