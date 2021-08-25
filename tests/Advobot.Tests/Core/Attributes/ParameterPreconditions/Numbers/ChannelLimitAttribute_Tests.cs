
using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.Numbers
{
	[TestClass]
	public sealed class ChannelLimitAttribute_Tests
		: ParameterPreconditionTestsBase<ChannelLimitAttribute>
	{
		protected override ChannelLimitAttribute Instance { get; } = new();

		[TestMethod]
		public async Task Standard_Test()
		{
			var expected = new Dictionary<int, bool>
			{
				{ -1, false },
				{ 0, true },
				{ 99, true },
				{ 100, false },
			};
			foreach (var kvp in expected)
			{
				var result = await CheckPermissionsAsync(kvp.Key).CAF();
				Assert.AreEqual(kvp.Value, result.IsSuccess);
			}
		}
	}
}