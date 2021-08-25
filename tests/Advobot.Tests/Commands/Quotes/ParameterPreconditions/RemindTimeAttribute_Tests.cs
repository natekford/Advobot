
using Advobot.Quotes.ParameterPreconditions;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Quotes.ParameterPreconditions
{
	[TestClass]
	public sealed class RemindTimeAttribute_Tests
		: ParameterPreconditionTestsBase<RemindTimeAttribute>
	{
		protected override RemindTimeAttribute Instance { get; } = new();

		[TestMethod]
		public async Task Standard_Test()
		{
			var expected = new Dictionary<int, bool>
			{
				{ 0, false },
				{ 1, true },
				{ 525600, true },
				{ 525601, false },
			};
			foreach (var kvp in expected)
			{
				var result = await CheckPermissionsAsync(kvp.Key).CAF();
				Assert.AreEqual(kvp.Value, result.IsSuccess);
			}
		}
	}
}