using Advobot.Tests.Fakes.Discord.Users;

using Discord;

namespace Advobot.Tests.Fakes.Discord;

public sealed class FakeApplication : FakeSnowflake, IApplication
{
	public int? ApproximateGuildCount => throw new NotImplementedException();
	public int? ApproximateUserAuthorizationCount => throw new NotImplementedException();
	public int? ApproximateUserInstallCount => throw new NotImplementedException();
	public bool BotRequiresCodeGrant { get; set; }
	public string CustomInstallUrl => throw new NotImplementedException();
	public string Description { get; set; } = "This is a fake application.";
	public ApplicationDiscoverabilityState DiscoverabilityState => throw new NotImplementedException();
	public DiscoveryEligibilityFlags DiscoveryEligibilityFlags => throw new NotImplementedException();
	public ApplicationExplicitContentFilterLevel ExplicitContentFilterLevel => throw new NotImplementedException();
	public ApplicationFlags Flags { get; set; }
	public PartialGuild Guild => throw new NotImplementedException();
	public string IconUrl { get; set; } = "";
	public ApplicationInstallParams InstallParams { get; set; }
	public IReadOnlyDictionary<ApplicationIntegrationType, ApplicationInstallParams> IntegrationTypesConfig => throw new NotImplementedException();
	public IReadOnlyCollection<string> InteractionEventTypes => throw new NotImplementedException();
	public string InteractionsEndpointUrl => throw new NotImplementedException();
	public ApplicationInteractionsVersion InteractionsVersion => throw new NotImplementedException();
	public bool IsBotPublic { get; set; }
	public bool IsHook => throw new NotImplementedException();
	public bool IsMonetized => throw new NotImplementedException();
	public ApplicationMonetizationEligibilityFlags MonetizationEligibilityFlags => throw new NotImplementedException();
	public ApplicationMonetizationState MonetizationState => throw new NotImplementedException();
	public string Name { get; set; } = "FakeBot";
	public IUser Owner { get; set; } = new FakeUser();
	public string PrivacyPolicy { get; set; }
	public IReadOnlyCollection<string> RedirectUris => throw new NotImplementedException();
	public string RoleConnectionsVerificationUrl => throw new NotImplementedException();
	public IReadOnlyCollection<string> RPCOrigins { get; set; } = [];
	public ApplicationRpcState RpcState => throw new NotImplementedException();
	public ApplicationStoreState StoreState => throw new NotImplementedException();
	public IReadOnlyCollection<string> Tags { get; set; } = [];
	public ITeam Team { get; set; }
	public string TermsOfService { get; set; }
	public ApplicationVerificationState VerificationState => throw new NotImplementedException();
	public string VerifyKey => throw new NotImplementedException();
	bool? IApplication.BotRequiresCodeGrant => throw new NotImplementedException();
	bool? IApplication.IsBotPublic => throw new NotImplementedException();
}