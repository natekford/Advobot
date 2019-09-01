using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;

using AdvorangesUtils;

using Discord;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels
{
	[TestClass]
	public sealed class CanModifyChannelAttribute_Tests
		: ParameterPreconditions_TestsBase<CanModifyChannelAttribute>
	{
		public override CanModifyChannelAttribute Instance { get; }
			= new CanModifyChannelAttribute(ChannelPermission.ManageChannels);

		[TestMethod]
		public async Task FailsOnNotIGuildChannel_Test()
			=> await AssertPreconditionFailsOnInvalidType(CheckAsync(1)).CAF();
	}
}