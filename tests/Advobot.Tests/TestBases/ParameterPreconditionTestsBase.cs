using AdvorangesUtils;

using Discord.Commands;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.TestBases
{
	public abstract class ParameterPreconditionTestsBase<T> : TestsBase
		 where T : ParameterPreconditionAttribute
	{
		protected abstract T Instance { get; }

		[TestMethod]
		public async Task InvalidType_Test()
			=> await AssertFailureAsync(new object()).CAF();

		protected async Task<PreconditionResult> AssertFailureAsync(
			object value,
			ParameterInfo? parameter = null)
		{
			var result = await Instance.CheckPermissionsAsync(Context, parameter, value, Services).CAF();
			Assert.IsFalse(result.IsSuccess);
			return result;
		}

		protected async Task<PreconditionResult> AssertSuccessAsync(
			object value,
			ParameterInfo? parameter = null)
		{
			var result = await Instance.CheckPermissionsAsync(Context, parameter, value, Services).CAF();
			Assert.IsTrue(result.IsSuccess);
			return result;
		}
	}
}