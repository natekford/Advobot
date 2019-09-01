using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

using Discord;

namespace Advobot.Tests.Fakes.Discord
{
	public class FakeUser : FakeSnowflake, IUser
	{
		public IImmutableSet<ClientType> ActiveClients => throw new NotImplementedException();
		public IActivity Activity => throw new NotImplementedException();
		public string AvatarId => throw new NotImplementedException();
		public string Discriminator => DiscriminatorValue.ToString();
		public ushort DiscriminatorValue { get; } = (ushort)new Random().Next(1, 10000);
		public bool IsBot { get; set; }
		public bool IsWebhook { get; set; }
		public string Mention => MentionUtils.MentionUser(Id);
		public UserStatus Status => throw new NotImplementedException();
		public string Username => "Mock User";

		public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
			=> CDN.GetUserAvatarUrl(Id, AvatarId, size, format);

		public string GetDefaultAvatarUrl()
			=> CDN.GetDefaultUserAvatarUrl(DiscriminatorValue);

		public Task<IDMChannel> GetOrCreateDMChannelAsync(RequestOptions options = null)
			=> throw new NotImplementedException();
	}
}