using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Discord;

namespace Advobot.Tests.Fakes.Discord
{
	public class FakeUser : FakeSnowflake, IUser
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public string AvatarId => throw new NotImplementedException();
		public string Discriminator => DiscriminatorValue.ToString();
		public ushort DiscriminatorValue { get; } = (ushort)new Random().Next(1, 10000);
		public bool IsBot { get; set; }
		public bool IsWebhook { get; set; }
		public string Username => "Mock User";
		public string Mention => MentionUtils.MentionUser(Id);
		public IActivity Activity => throw new NotImplementedException();
		public UserStatus Status => throw new NotImplementedException();
		public IImmutableSet<ClientType> ActiveClients => throw new NotImplementedException();

		public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
			=> CDN.GetUserAvatarUrl(Id, AvatarId, size, format);
		public string GetDefaultAvatarUrl()
			=> CDN.GetDefaultUserAvatarUrl(DiscriminatorValue);
		public Task<IDMChannel> GetOrCreateDMChannelAsync(RequestOptions options = null)
			=> throw new NotImplementedException();
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
	}
}
