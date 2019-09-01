using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Emotes;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.DiscordObjectValidation.Emotes
{
	[TestClass]
	public sealed class HasRequiredRolesAttribute_Tests
		: ParameterlessParameterPreconditions_TestsBase<HasRequiredRolesAttribute>
	{
		[TestMethod]
		public async Task FailsOnNotGuildEmote_Test()
			=> await AssertPreconditionFailsOnInvalidType(CheckAsync(1)).CAF();
	}
}