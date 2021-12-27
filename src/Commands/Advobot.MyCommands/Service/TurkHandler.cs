using Advobot.MyCommands.Database;

using AdvorangesUtils;

using DetectLanguage;

using Discord;
using Discord.WebSocket;

using System.Net;

namespace Advobot.MyCommands.Service;

public sealed class TurkHandler
{
	private static readonly TimeSpan _DontBanIfOlderThan = TimeSpan.FromDays(7);
	private readonly IMyCommandsDatabase _Db;
	private readonly HashSet<ulong> _Ids = new();
	private string? _APIKey;
	private DetectLanguageClient? _DetectLanguage;

	public TurkHandler(BaseSocketClient client, IMyCommandsDatabase db)
	{
		_Db = db;

		client.GuildMemberUpdated += OnGuildMemberUpdated;
	}

	private async Task OnGuildMemberUpdated(
		Cacheable<SocketGuildUser, ulong> _,
		SocketGuildUser user)
	{
		if (user.Guild.Id != 199339772118827008
			|| user.Activities.OfType<CustomStatusGame>().SingleOrDefault()
				is not CustomStatusGame status // Need a custom status
			|| status.State is null // Need custom text
			|| !user.JoinedAt.HasValue // If no join date then assume old user
			|| DateTime.UtcNow - user.JoinedAt?.UtcDateTime > _DontBanIfOlderThan
			|| _Ids.Contains(user.Id))
		{
			return;
		}

		var config = await _Db.GetDetectLanguageConfigAsync().CAF();
		if (config.APIKey == null
			|| config.CooldownStart?.Day == DateTime.UtcNow.Day)
		{
			// If no API key set, nothing we can do
			// If rate limited, wait until next day
			return;
		}

		if (_APIKey != config.APIKey)
		{
			// If API key changed, make new client
			_APIKey = config.APIKey;
			_DetectLanguage = new(_APIKey);
		}

		DetectResult[] languages;
		try
		{
			languages = await _DetectLanguage!.DetectAsync(status.State).CAF();
		}
		catch (DetectLanguageException dle) when (dle.StatusCode == HttpStatusCode.PaymentRequired)
		{
			// Ratelimit
			await _Db.UpsertDetectLanguageConfigAsync(config with
			{
				CooldownStartTicks = DateTime.UtcNow.Ticks,
			}).CAF();
			return;
		}
		catch
		{
			// Some other error, probably a 500. Nothing we can do, so leave
			return;
		}

		_Ids.Add(user.Id);
		if (languages.Any(x => x.confidence > config.ConfidenceLimit && x.language == "tr"))
		{
			var reason = @$"turkish activity so probable spammer (""{status.State}"").";
			await user.BanAsync(reason: reason).CAF();
		}
	}
}