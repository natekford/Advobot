using Advobot.ParameterPreconditions.Numbers;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Core.ParameterPreconditions.Numbers;

[TestClass]
public sealed class ChannelBitrate_Tests : ParameterPrecondition_Tests<ChannelBitrate>
{
	protected override ChannelBitrate Instance { get; } = new();

	[TestMethod]
	public async Task Value129_Test()
	{
		const int VALUE = 129;
		Context.Guild.PremiumSubscriptionCount = 0;
		await AssertFailureAsync(VALUE).ConfigureAwait(false);
		Context.Guild.PremiumSubscriptionCount = 2;
		await AssertFailureAsync(VALUE).ConfigureAwait(false);
		Context.Guild.PremiumSubscriptionCount = 10;
		await AssertSuccessAsync(VALUE).ConfigureAwait(false);
		Context.Guild.PremiumSubscriptionCount = 50;
		await AssertSuccessAsync(VALUE).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task Value257_Test()
	{
		const int VALUE = 257;
		Context.Guild.PremiumSubscriptionCount = 0;
		await AssertFailureAsync(VALUE).ConfigureAwait(false);
		Context.Guild.PremiumSubscriptionCount = 2;
		await AssertFailureAsync(VALUE).ConfigureAwait(false);
		Context.Guild.PremiumSubscriptionCount = 10;
		await AssertFailureAsync(VALUE).ConfigureAwait(false);
		Context.Guild.PremiumSubscriptionCount = 50;
		await AssertSuccessAsync(VALUE).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task Value385_Test()
	{
		const int VALUE = 385;
		Context.Guild.PremiumSubscriptionCount = 0;
		await AssertFailureAsync(VALUE).ConfigureAwait(false);
		Context.Guild.PremiumSubscriptionCount = 2;
		await AssertFailureAsync(VALUE).ConfigureAwait(false);
		Context.Guild.PremiumSubscriptionCount = 10;
		await AssertFailureAsync(VALUE).ConfigureAwait(false);
		Context.Guild.PremiumSubscriptionCount = 50;
		await AssertFailureAsync(VALUE).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task Value7_Test()
		=> await AssertFailureAsync(7).ConfigureAwait(false);

	[TestMethod]
	public async Task Value8_Test()
		=> await AssertSuccessAsync(8).ConfigureAwait(false);

	[TestMethod]
	public async Task Value96_Test()
		=> await AssertSuccessAsync(96).ConfigureAwait(false);

	[TestMethod]
	public async Task Value97_Test()
	{
		const int VALUE = 97;
		Context.Guild.PremiumSubscriptionCount = 0;
		await AssertFailureAsync(VALUE).ConfigureAwait(false);
		Context.Guild.PremiumSubscriptionCount = 2;
		await AssertSuccessAsync(VALUE).ConfigureAwait(false);
		Context.Guild.PremiumSubscriptionCount = 10;
		await AssertSuccessAsync(VALUE).ConfigureAwait(false);
		Context.Guild.PremiumSubscriptionCount = 50;
		await AssertSuccessAsync(VALUE).ConfigureAwait(false);
	}
}