using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.Fakes.Discord.Users;

using Discord;

namespace Advobot.Tests.Fakes.Discord;

public sealed class FakeInviteMetadata : IInviteMetadata
{
	public ulong ChannelId => FakeChannel.Id;
	public string ChannelName => FakeChannel.Name;

	public ChannelType ChannelType => FakeChannel switch
	{
		ITextChannel _ => ChannelType.Text,
		IVoiceChannel _ => ChannelType.Voice,
		ICategoryChannel _ => ChannelType.Category,
		_ => throw new ArgumentOutOfRangeException(nameof(FakeChannel)),
	};

	public string Code { get; set; }
	public DateTimeOffset? CreatedAt { get; } = DateTimeOffset.UtcNow;
	public FakeGuildChannel FakeChannel { get; }
	public FakeGuild FakeGuild => FakeChannel.FakeGuild;
	public FakeGuildUser FakeInviter { get; }
	public ulong? GuildId => FakeGuild.Id;
	public string GuildName => FakeGuild.Name;
	public string Id { get; set; }
	public bool IsRevoked { get; set; }
	public bool IsTemporary { get; set; }
	public int? MaxAge { get; set; }
	public int? MaxUses { get; set; }
	public int? MemberCount => FakeGuild.FakeUsers.Count;
	public int? PresenceCount => FakeGuild.FakeUsers.Count(x => x.Status == UserStatus.Online);
	public TargetUserType TargetUserType { get; set; }
	public string Url => $"https://discord.gg/{Id}";
	public int? Uses { get; set; }
	IChannel IInvite.Channel => FakeChannel;
	IGuild IInvite.Guild => FakeGuild;
	IUser IInvite.Inviter => FakeInviter;
	IUser IInvite.TargetUser => throw new NotImplementedException();

	public FakeInviteMetadata(FakeGuildChannel channel, FakeGuildUser inviter)
	{
		FakeChannel = channel;
		FakeInviter = inviter;
		FakeGuild.FakeInvites.Add(this);
		Code = Id = GenerateRandomInviteLink();
	}

	public Task DeleteAsync(RequestOptions? options = null)
	{
		FakeGuild.FakeInvites.Remove(this);
		return Task.CompletedTask;
	}

	private static string GenerateRandomInviteLink()
	{
		const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

		var stringChars = new char[5];
		var random = new Random();

		for (var i = 0; i < stringChars.Length; i++)
		{
			stringChars[i] = CHARS[random.Next(CHARS.Length)];
		}
		return new(stringChars);
	}
}