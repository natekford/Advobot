
using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.Numbers
{
	[TestClass]
	public sealed class GuildAfkTimeAttribute_Tests
		: ParameterPreconditionTestsBase<GuildAfkTimeAttribute>
	{
		protected override GuildAfkTimeAttribute Instance { get; } = new();

		[TestMethod]
		public async Task Standard_Test()
		{
			var expected = new Dictionary<int, bool>
			{
				{ 59, false },
				{ 60, true },
				{ 300, true },
				{ 900, true },
				{ 1800, true },
				{ 3600, true },
				{ 3601, false },
			};
			foreach (var kvp in expected)
			{
				var result = await CheckPermissionsAsync(kvp.Key).CAF();
				Assert.AreEqual(kvp.Value, result.IsSuccess);
			}
		}
	}
}