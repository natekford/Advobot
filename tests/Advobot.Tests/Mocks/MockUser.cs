using Discord;
using System;
using System.Threading.Tasks;

namespace Advobot.Tests.Mocks
{
	public class MockUser : IUser
	{
		public string AvatarId => throw new NotImplementedException();
		public string Discriminator => DiscriminatorValue.ToString();
		public ushort DiscriminatorValue { get; } = (ushort)new Random().Next(1, 10000);
		public bool IsBot { get; set; }
		public bool IsWebhook { get; set; }
		public string Username => "Mock User";
		public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(Id);
		public ulong Id { get; }
		public string Mention => MentionUtils.MentionUser(Id);
		public IActivity Activity => throw new NotImplementedException();
		public UserStatus Status => throw new NotImplementedException();

		public MockUser(ulong id)
		{
			Id = id;
		}

		public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
			=> CDN.GetUserAvatarUrl(Id, AvatarId, size, format);
		public string GetDefaultAvatarUrl()
			=> CDN.GetDefaultUserAvatarUrl(DiscriminatorValue);
		public Task<IDMChannel> GetOrCreateDMChannelAsync(RequestOptions options = null)
			=> throw new NotImplementedException();
	}
}
