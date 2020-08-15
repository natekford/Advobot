using System.Threading.Tasks;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.TestBases
{
	public abstract class ParameterPreconditionTestsBase : TestsBase
	{
		protected abstract ParameterPreconditionAttribute Instance { get; }

		[TestMethod]
		public async Task InvalidType_Test()
		{
			var result = await CheckPermissionsAsync(new object()).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		protected Task<PreconditionResult> CheckPermissionsAsync(object value, ParameterInfo? parameter = null)
			=> Instance.CheckPermissionsAsync(Context, parameter, value, Services);
	}
}