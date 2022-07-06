using AdvorangesUtils;

using Discord;
using Discord.WebSocket;

namespace Advobot.MyCommands.Service;

public sealed class Ashman99ReactionHandler
{
	private const int AMOUNT_ALLOWED = 2;
	private const ulong ASH_ID = 181981729479852032;
	private const ulong TED_SERVER_ID = 198857069586022400;

	private static readonly RequestOptions _ReactionRemoval = new()
	{
		AuditLogReason = "read if ted noob."
	};
	private static readonly TimeSpan _Window = TimeSpan.FromMinutes(1);

	private int _Amount;
	private DateTime _End = DateTime.UtcNow;

	public Ashman99ReactionHandler(BaseSocketClient client)
	{
		client.ReactionAdded += OnReactionAdded;
	}

	private async Task OnReactionAdded(
		Cacheable<IUserMessage, ulong> message,
		Cacheable<IMessageChannel, ulong> channel,
		SocketReaction reaction)
	{
		if (reaction.UserId != ASH_ID
			|| await channel.GetOrDownloadAsync().CAF() is not IGuildChannel chn
			|| chn.Guild.Id != TED_SERVER_ID
			|| ++_Amount <= AMOUNT_ALLOWED)
		{
			return;
		}

		var now = DateTime.UtcNow;
		if (now > _End)
		{
			_Amount = 1;
			_End = now + _Window;
			return;
		}

		var msg = await message.GetOrDownloadAsync().CAF();
		await msg.RemoveReactionAsync(reaction.Emote, reaction.UserId, _ReactionRemoval).CAF();
	}
}