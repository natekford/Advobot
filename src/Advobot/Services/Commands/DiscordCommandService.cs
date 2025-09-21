using Advobot.Modules;
using Advobot.Services.BotConfig;
using Advobot.Services.Events;
using Advobot.Services.GuildSettings;

using Discord;

using System.Collections.Concurrent;
using System.Globalization;

namespace Advobot.Services.Commands;

/// <summary>
/// Executes commands from Discord messages.
/// </summary>
/// <param name="services"></param>
/// <param name="commandService"></param>
/// <param name="client"></param>
/// <param name="eventProvider"></param>
/// <param name="botConfig"></param>
/// <param name="guildSettings"></param>
public class DiscordCommandService(
	IServiceProvider services,
	AdvobotCommandService commandService,
	IDiscordClient client,
	EventProvider eventProvider,
	IRuntimeConfig botConfig,
	IGuildSettingsService guildSettings
) : StartableService
{
	private readonly ConcurrentDictionary<ulong, byte> _GatheringUsers = new();

	/// <inheritdoc />
	protected override Task StartAsyncImpl()
	{
		eventProvider.MessageReceived.Add(OnMessageReceived);

		return Task.CompletedTask;
	}

	/// <inheritdoc />
	protected override Task StopAsyncImpl()
	{
		eventProvider.MessageReceived.Remove(OnMessageReceived);

		return base.StopAsyncImpl();
	}

	private async Task OnMessageReceived(IMessage message)
	{
		if (botConfig.Pause
			|| message.Author.IsBot
			|| string.IsNullOrWhiteSpace(message.Content)
			|| botConfig.UsersIgnoredFromCommands.Contains(message.Author.Id)
			|| message is not IUserMessage msg
			|| msg.Author is not IGuildUser user
			|| msg.Channel is not ITextChannel _)
		{
			return;
		}

		if (_GatheringUsers.TryAdd(user.Guild.Id, 0))
		{
			_ = user.Guild.DownloadUsersAsync();
		}

		var content = msg.Content;
		var mention = client.CurrentUser.Mention;
		if (content.StartsWith(mention))
		{
			content = content[mention.Length..];
		}
		else
		{
			var prefix = await guildSettings.GetPrefixAsync(user.Guild).ConfigureAwait(false);
			if (content.StartsWith(prefix))
			{
				content = content[prefix.Length..];
			}
			else
			{
				return;
			}
		}

		var culture = await guildSettings.GetCultureAsync(user.Guild).ConfigureAwait(false);
		CultureInfo.CurrentUICulture = culture;
		CultureInfo.CurrentCulture = culture;

		var context = new GuildContext(services, client, msg);
		await commandService.InitializeAsync().ConfigureAwait(false);
		await commandService.ExecuteAsync(context, content).ConfigureAwait(false);
	}
}