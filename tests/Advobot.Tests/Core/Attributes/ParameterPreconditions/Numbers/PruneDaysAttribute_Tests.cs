
using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.Numbers
{
	[TestClass]
	public sealed class PruneDaysAttribute_Tests
		: ParameterPreconditionTestsBase<PruneDaysAttribute>
	{
		protected override PruneDaysAttribute Instance { get; } = new();

		[TestMethod]
		public async Task Standard_Test()
		{
			var expected = new Dictionary<int, bool>
			{
				{ 0, false },
				{ 1, true },
				{ 7, true },
				{ 30, true },
				{ 31, false },
			};
			foreach (var kvp in expected)
			{
				var result = await CheckPermissionsAsync(kvp.Key).CAF();
				Assert.AreEqual(kvp.Value, result.IsSuccess);
			}
		}
	}
}