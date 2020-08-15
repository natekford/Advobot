using System;
using System.Threading.Tasks;

using Advobot.Attributes.Preconditions;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.Preconditions
{
	[Obsolete]
	[TestClass]
	public sealed class DisabledCommandAttribute_Tests : PreconditionTestsBase
	{
		protected override PreconditionAttribute Instance { get; }
			= new DisabledCommandAttribute();

		[TestMethod]
		public async Task NeverWorks_Test()
		{
			var result = await CheckPermissionsAsync().CAF();
			Assert.IsFalse(result.IsSuccess);
		}
	}
}