using Advobot.Standard.Commands;
using Advobot.Tests.Fakes.Discord.Users;
using Advobot.Tests.TestBases;
using Advobot.Utilities;

using Discord;

namespace Advobot.Tests.Commands.Standard;

[TestClass]
public sealed class Misc_Tests : Command_Tests
{
	private IBan Ban { get; set; }
	private GuildEmote Emote { get; set; }
	private IInviteMetadata Invite { get; set; }
	private IRole Role { get; set; }
	private IWebhook Webhook { get; set; }

	[TestMethod]
	public async Task GetBan_Test()
	{
		var input = $"{nameof(Misc.Get)} {nameof(Misc.Get.Ban)} {Ban}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(typeof(IBan), result.Command.Parameters.Single().ParameterType);
	}

	[TestMethod]
	public async Task GetBanImplicit_Test()
	{
		var input = $"{nameof(Misc.Get)} {Ban}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(typeof(IBan), result.Command.Parameters.Single().ParameterType);
	}

	[TestMethod]
	public async Task GetBot_Test()
	{
		var input = $"{nameof(Misc.Get)} {nameof(Misc.Get.Bot)}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsEmpty(result.Command.Parameters);
	}

	[TestMethod]
	public async Task GetChannel_Test()
	{
		var input = $"{nameof(Misc.Get)} {nameof(Misc.Get.Channel)} {Context.Channel}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(typeof(IGuildChannel), result.Command.Parameters.Single().ParameterType);
	}

	[TestMethod]
	public async Task GetChannelImplicit_Test()
	{
		var input = $"{nameof(Misc.Get)} {Context.Channel}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(typeof(IGuildChannel), result.Command.Parameters.Single().ParameterType);
	}

	[TestMethod]
	public async Task GetEmote_Test()
	{
		var input = $"{nameof(Misc.Get)} {nameof(Misc.Get.Emote)} {Emote}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(typeof(Emote), result.Command.Parameters.Single().ParameterType);
	}

	[TestMethod]
	public async Task GetEmoteImplicit_Test()
	{
		var input = $"{nameof(Misc.Get)} {Emote}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(typeof(Emote), result.Command.Parameters.Single().ParameterType);
	}

	[TestMethod]
	public async Task GetFailure_Test()
	{
		var input = $"{nameof(Misc.Get)} asdf";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.AreEqual(typeof(string), result.Command.Parameters.Single().ParameterType);
	}

	[TestMethod]
	public async Task GetGuild_Test()
	{
		var input = $"{nameof(Misc.Get)} {nameof(Misc.Get.Guild)}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsEmpty(result.Command.Parameters);
	}

	[TestMethod]
	public async Task GetInvite_Test()
	{
		var input = $"{nameof(Misc.Get)} {nameof(Misc.Get.Invite)} {Invite}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(typeof(IInviteMetadata), result.Command.Parameters.Single().ParameterType);
	}

	[TestMethod]
	public async Task GetInviteImplicit_Test()
	{
		var input = $"{nameof(Misc.Get)} {Invite}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(typeof(IInviteMetadata), result.Command.Parameters.Single().ParameterType);
	}

	[TestMethod]
	public async Task GetRole_Test()
	{
		var input = $"{nameof(Misc.Get)} {nameof(Misc.Get.Role)} {Role}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(typeof(IRole), result.Command.Parameters.Single().ParameterType);
	}

	[TestMethod]
	public async Task GetRoleImplicit_Test()
	{
		var input = $"{nameof(Misc.Get)} {Role}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(typeof(IRole), result.Command.Parameters.Single().ParameterType);
	}

	[TestMethod]
	public async Task GetUser_Test()
	{
		var input = $"{nameof(Misc.Get)} {nameof(Misc.Get.User)} {Context.User}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(typeof(IGuildUser), result.Command.Parameters.Single().ParameterType);
	}

	[TestMethod]
	public async Task GetUserImplicit_Test()
	{
		var input = $"{nameof(Misc.Get)} {Context.User}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(typeof(IGuildUser), result.Command.Parameters.Single().ParameterType);
	}

	[TestMethod]
	public async Task GetWebhook_Test()
	{
		var input = $"{nameof(Misc.Get)} {nameof(Misc.Get.Webhook)} {Webhook}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(typeof(IWebhook), result.Command.Parameters.Single().ParameterType);
	}

	[TestMethod]
	public async Task GetWebhookImplicit_Test()
	{
		var input = $"{nameof(Misc.Get)} {Webhook}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(typeof(IWebhook), result.Command.Parameters.Single().ParameterType);
	}

	protected override async Task SetupAsync()
	{
		await base.SetupAsync().ConfigureAwait(false);

		var user = new FakeGuildUser(Context.Guild);
		await Context.Guild.AddBanAsync(user).ConfigureAwait(false);
		Ban = await Context.Guild.GetBanAsync(user).ConfigureAwait(false);
		Emote = await Context.Guild.CreateEmoteAsync("meow", new()).ConfigureAwait(false);
		Invite = await Context.Channel.CreateInviteAsync().ConfigureAwait(false);
		Role = await Context.Guild.CreateEmptyRoleAsync("rool").ConfigureAwait(false);
		Webhook = await Context.Channel.CreateWebhookAsync("woof").ConfigureAwait(false);
	}
}