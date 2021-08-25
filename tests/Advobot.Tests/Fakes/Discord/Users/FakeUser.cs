using System.Collections.Immutable;

using Discord;

namespace Advobot.Tests.Fakes.Discord.Users
{
	public class FakeUser : FakeSnowflake, IUser
	{
		public IImmutableSet<ClientType> ActiveClients => throw new NotImplementedException();
		public IImmutableList<IActivity> Activities => throw new NotImplementedException();
		public IActivity Activity => throw new NotImplementedException();
		public string AvatarId => throw new NotImplementedException();
		public string Discriminator => DiscriminatorValue.ToString();
		public ushort DiscriminatorValue { get; set; } = (ushort)new Random().Next(1, 10000);
		public bool IsBot { get; set; }
		public bool IsWebhook { get; set; }
		public string Mention => MentionUtils.MentionUser(Id);
		public UserProperties? PublicFlags => throw new NotImplementedException();
		public UserStatus Status => throw new NotImplementedException();
		public string Username { get; set; } = "Fake User";

		public Task<IDMChannel> CreateDMChannelAsync(RequestOptions options = null)
			=> throw new NotImplementedException();

		public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
			=> CDN.GetUserAvatarUrl(Id, AvatarId, size, format);

		public string GetDefaultAvatarUrl()
			=> CDN.GetDefaultUserAvatarUrl(DiscriminatorValue);
	}
}