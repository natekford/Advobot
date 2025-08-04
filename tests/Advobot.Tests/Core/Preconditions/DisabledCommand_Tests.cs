using Advobot.Preconditions;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

namespace Advobot.Tests.Core.Preconditions;

[Obsolete("asdf")]
[TestClass]
public sealed class DisabledCommand_Tests : Precondition_Tests<DisabledCommand>
{
	protected override DisabledCommand Instance { get; } = new();

	[TestMethod]
	public async Task NeverWorks_Test()
	{
		var result = await CheckPermissionsAsync().CAF();
		Assert.IsFalse(result.IsSuccess);
	}
}