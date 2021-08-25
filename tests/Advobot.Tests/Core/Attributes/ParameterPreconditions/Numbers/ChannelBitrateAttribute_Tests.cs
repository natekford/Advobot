using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.Numbers
{
	[TestClass]
	public sealed class ChannelBitrateAttribute_Tests
		: ParameterPreconditionTestsBase<ChannelBitrateAttribute>
	{
		protected override ChannelBitrateAttribute Instance { get; } = new();

		[TestMethod]
		public async Task Value129_Test()
		{
			const int VALUE = 129;
			Context.Guild.PremiumSubscriptionCount = 0;
			await AssertFailureAsync(VALUE).CAF();
			Context.Guild.PremiumSubscriptionCount = 2;
			await AssertFailureAsync(VALUE).CAF();
			Context.Guild.PremiumSubscriptionCount = 10;
			await AssertSuccessAsync(VALUE).CAF();
			Context.Guild.PremiumSubscriptionCount = 50;
			await AssertSuccessAsync(VALUE).CAF();
		}

		[TestMethod]
		public async Task Value257_Test()
		{
			const int VALUE = 257;
			Context.Guild.PremiumSubscriptionCount = 0;
			await AssertFailureAsync(VALUE).CAF();
			Context.Guild.PremiumSubscriptionCount = 2;
			await AssertFailureAsync(VALUE).CAF();
			Context.Guild.PremiumSubscriptionCount = 10;
			await AssertFailureAsync(VALUE).CAF();
			Context.Guild.PremiumSubscriptionCount = 50;
			await AssertSuccessAsync(VALUE).CAF();
		}

		[TestMethod]
		public async Task Value385_Test()
		{
			const int VALUE = 385;
			Context.Guild.PremiumSubscriptionCount = 0;
			await AssertFailureAsync(VALUE).CAF();
			Context.Guild.PremiumSubscriptionCount = 2;
			await AssertFailureAsync(VALUE).CAF();
			Context.Guild.PremiumSubscriptionCount = 10;
			await AssertFailureAsync(VALUE).CAF();
			Context.Guild.PremiumSubscriptionCount = 50;
			await AssertFailureAsync(VALUE).CAF();
		}

		[TestMethod]
		public async Task Value7_Test()
			=> await AssertFailureAsync(7).CAF();

		[TestMethod]
		public async Task Value8_Test()
			=> await AssertSuccessAsync(8).CAF();

		[TestMethod]
		public async Task Value96_Test()
			=> await AssertSuccessAsync(96).CAF();

		[TestMethod]
		public async Task Value97_Test()
		{
			const int VALUE = 97;
			Context.Guild.PremiumSubscriptionCount = 0;
			await AssertFailureAsync(VALUE).CAF();
			Context.Guild.PremiumSubscriptionCount = 2;
			await AssertSuccessAsync(VALUE).CAF();
			Context.Guild.PremiumSubscriptionCount = 10;
			await AssertSuccessAsync(VALUE).CAF();
			Context.Guild.PremiumSubscriptionCount = 50;
			await AssertSuccessAsync(VALUE).CAF();
		}
	}
}