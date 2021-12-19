using Advobot.Tests.Fakes.Discord.Users;

using Discord;

namespace Advobot.Tests.Fakes.Discord;

public sealed class FakeApplication : FakeSnowflake, IApplication
{
	public bool BotRequiresCodeGrant { get; set; }
	public string Description { get; set; } = "This is a fake application.";
	public ApplicationFlags Flags { get; set; }
	public string IconUrl { get; set; } = "";
	public ApplicationInstallParams InstallParams { get; set; }
	public bool IsBotPublic { get; set; }
	public string Name { get; set; } = "FakeBot";
	public IUser Owner { get; set; } = new FakeUser();
	public string PrivacyPolicy { get; set; }
	public IReadOnlyCollection<string> RPCOrigins { get; set; } = Array.Empty<string>();
	public IReadOnlyCollection<string> Tags { get; set; } = Array.Empty<string>();
	public ITeam Team { get; set; }
	public string TermsOfService { get; set; }
}