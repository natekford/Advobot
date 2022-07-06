using AdvorangesUtils;

using Discord;
using Discord.WebSocket;

namespace Advobot.MyCommands.Service;

public sealed class Ashman99ReactionHandler
{
	private static readonly RequestOptions ReactionRemoval = new()
	{
		AuditLogReason = "read if ted noob."
	};

	public Ashman99ReactionHandler(BaseSocketClient client)
	{
		client.ReactionAdded += Client_ReactionAdded;
	}

	private async Task Client_ReactionAdded(
		Cacheable<IUserMessage, ulong> message,
		Cacheable<IMessageChannel, ulong> channel,
		SocketReaction reaction)
	{
		if (reaction.UserId != 181981729479852032)
		{
			return;
		}

		var chn = await channel.GetOrDownloadAsync().CAF();
		if (chn is not IGuildChannel guildChannel
			|| guildChannel.GuildId != 198857069586022400)
		{
			return;
		}

		var msg = await message.GetOrDownloadAsync().CAF();
		await msg.RemoveReactionAsync(reaction.Emote, reaction.UserId, ReactionRemoval).CAF();
	}
}