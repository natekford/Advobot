using Advobot.MyCommands.Database;
using Advobot.Services;
using Advobot.Services.Events;
using Advobot.Services.Time;

using DetectLanguage;

using Discord;

using Microsoft.Extensions.Logging;

using System.Net;

namespace Advobot.MyCommands.Service;

public sealed class SpammerHandler(
	ILogger<SpammerHandler> logger,
	MyCommandsDatabase db,
	EventProvider eventProvider,
	TimeProvider time
) : StartableService
{
	private static readonly TimeSpan _DontBanIfOlderThan = TimeSpan.FromDays(7);
	private readonly HashSet<ulong> _Ids = [];
	private string? _APIKey;
	private DetectLanguageClient? _DetectLanguage;

	protected override Task StartAsyncImpl()
	{
		eventProvider.GuildMemberUpdated.Add(OnGuildMemberUpdated);

		return Task.CompletedTask;
	}

	protected override Task StopAsyncImpl()
	{
		eventProvider.GuildMemberUpdated.Remove(OnGuildMemberUpdated);

		return base.StopAsyncImpl();
	}

	private async Task OnGuildMemberUpdated(IGuildUser? _, IGuildUser user)
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

		var config = await db.GetDetectLanguageConfigAsync().ConfigureAwait(false);
		if (config.APIKey is null
			|| config.CooldownStart?.Day == time.GetUtcNow().Day)
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
			languages = await _DetectLanguage!.DetectAsync(status.State).ConfigureAwait(false);
		}
		// Ratelimit
		catch (DetectLanguageException dle) when (dle.StatusCode == HttpStatusCode.PaymentRequired)
		{
			await db.UpsertDetectLanguageConfigAsync(config with
			{
				CooldownStartTicks = time.GetUtcNow().Ticks,
			}).ConfigureAwait(false);

			logger.LogWarning(
				message: "DetectLanguage rate limit reached."
			);
			return;
		}
		// Some other error, nothing we can do
		catch (Exception e)
		{
			logger.LogWarning(
				exception: e,
				message: "Exception occurred while using DetectLanguage."
			);
			return;
		}

		_Ids.Add(user.Id);
		if (languages.Any(x => x.confidence > config.ConfidenceLimit && x.language == "tr"))
		{
			var reason = @$"turkish activity so probable spammer (""{status.State}"").";
			await user.BanAsync(reason: reason).ConfigureAwait(false);

			logger.LogInformation(
				message: "Banned probable spammer {User}.",
				args: [user.Id]
			);
		}
	}
}