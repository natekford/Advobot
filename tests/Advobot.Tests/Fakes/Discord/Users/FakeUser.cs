using Discord;

namespace Advobot.Tests.Fakes.Discord.Users;

public class FakeUser : FakeSnowflake, IUser
{
	public IReadOnlyCollection<ClientType> ActiveClients => throw new NotImplementedException();
	public IReadOnlyCollection<IActivity> Activities => FakeActivities;
	public IActivity Activity => Activities.FirstOrDefault()!;
	public string AvatarDecorationHash => throw new NotImplementedException();
	public ulong? AvatarDecorationSkuId => throw new NotImplementedException();
	public string AvatarId { get; set; } = "unknown";
	public string Discriminator => DiscriminatorValue.ToString();
	public ushort DiscriminatorValue { get; set; } = (ushort)new Random().Next(1, 10000);
	public List<IActivity> FakeActivities { get; set; } = [];
	public string GlobalName { get; set; } = "Fake Global Name";
	public bool IsBot { get; set; }
	public bool IsWebhook { get; set; }
	public string Mention => MentionUtils.MentionUser(Id);
	public PrimaryGuild? PrimaryGuild => throw new NotImplementedException();
	public UserProperties? PublicFlags => throw new NotImplementedException();
	public UserStatus Status { get; set; } = UserStatus.Online;
	public string Username { get; set; } = "Fake User";

	public Task<IDMChannel> CreateDMChannelAsync(RequestOptions options = null)
		=> throw new NotImplementedException();

	public string GetAvatarDecorationUrl()
		=> throw new NotImplementedException();

	public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
		=> CDN.GetUserAvatarUrl(Id, AvatarId, size, format);

	public string GetDefaultAvatarUrl()
		=> CDN.GetDefaultUserAvatarUrl(DiscriminatorValue);

	public string GetDisplayAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
		=> GetAvatarUrl(format, size);
}