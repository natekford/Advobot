using Discord;
using Discord.Audio;

namespace Advobot.Tests.Fakes.Discord.Channels;

public sealed class FakeVoiceChannel(FakeGuild guild)
	: FakeTextChannel(guild), IVoiceChannel
{
	public int Bitrate { get; set; }
	public string RTCRegion => throw new NotImplementedException();
	public int? UserLimit { get; set; }
	public VideoQualityMode VideoQualityMode { get; set; }

	public Task<IAudioClient> ConnectAsync(bool selfDeaf = false, bool selfMute = false, bool external = false, bool disconnect = true)
		=> throw new NotImplementedException();

	public Task DisconnectAsync()
		=> throw new NotImplementedException();

	public Task ModifyAsync(Action<VoiceChannelProperties> func, RequestOptions? options = null)
	{
		ModifyAsync((Action<GuildChannelProperties>)func);

		var args = new VoiceChannelProperties();
		func(args);

		Bitrate = args.Bitrate.GetValueOrDefault(Bitrate);
		UserLimit = args.UserLimit.GetValueOrDefault(UserLimit);

		return Task.CompletedTask;
	}

	public Task ModifyAsync(Action<AudioChannelProperties> func, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task SetStatusAsync(string status, RequestOptions options = null)
		=> throw new NotImplementedException();
}