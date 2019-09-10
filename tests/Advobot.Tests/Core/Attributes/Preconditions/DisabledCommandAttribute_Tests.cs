using System;
using System.Threading.Tasks;

using Advobot.Attributes.Preconditions;
using Advobot.Tests.PreconditionTestsBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.Preconditions
{
	[Obsolete]
	[TestClass]
	public sealed class DisabledCommandAttribute_Tests
		: ParameterlessPreconditions_TestBase<DisabledCommandAttribute>
	{
		[TestMethod]
		public async Task NeverWorks_Test()
		{
			var result = await CheckAsync().CAF();
			Assert.IsFalse(result.IsSuccess);
		}
	}
}