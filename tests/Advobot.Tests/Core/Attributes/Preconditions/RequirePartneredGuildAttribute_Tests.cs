using System.Threading.Tasks;

using Advobot.Attributes.Preconditions;
using Advobot.Tests.PreconditionTestsBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.Preconditions
{
	[TestClass]
	public sealed class RequirePartneredGuildAttribute_Tests
		: ParameterlessPreconditions_TestBase<RequirePartneredGuildAttribute>
	{
		[TestMethod]
		public async Task IsNotPartnered_Test()
		{
			var result = await CheckAsync().CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task IsPartnered_Test()
		{
			Context.Guild.Features.Add("a feature");

			var result = await CheckAsync().CAF();
			Assert.IsTrue(result.IsSuccess);
		}
	}
}