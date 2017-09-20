using Discord;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Actions
{
	public static class GuildActions
	{
		public static bool HasGlobalEmotes(this IGuild guild)
		{
			return guild.Emotes.Any(x => x.IsManaged && x.RequireColons);
		}
		public static IGuild GetGuild(this IMessage message)
		{
			return (message?.Channel as IGuildChannel)?.Guild;
		}
		public static IGuild GetGuild(this IUser user)
		{
			return (user as IGuildUser)?.Guild;
		}
		public static IGuild GetGuild(this IChannel channel)
		{
			return (channel as IGuildChannel)?.Guild;
		}
		public static IGuild GetGuild(this IRole role)
		{
			return role?.Guild;
		}

		public static async Task<int> PruneUsers(IGuild guild, int days, bool simulate, string reason)
		{
			return await guild.PruneUsersAsync(days, simulate, new RequestOptions { AuditLogReason = reason });
		}

		public static async Task ModifyGuildName(IGuild guild, string name, string reason)
		{
			await guild.ModifyAsync(x => x.Name = name, new RequestOptions { AuditLogReason = reason });
		}
		public static async Task ModifyGuildRegion(IGuild guild, string region, string reason)
		{
			await guild.ModifyAsync(x => x.RegionId = region, new RequestOptions { AuditLogReason = reason });
		}
		public static async Task ModifyGuildAFKTime(IGuild guild, int time, string reason)
		{
			await guild.ModifyAsync(x => x.AfkTimeout = time, new RequestOptions { AuditLogReason = reason });
		}
		public static async Task ModifyGuildAFKChannel(IGuild guild, IVoiceChannel channel, string reason)
		{
			await guild.ModifyAsync(x => x.AfkChannel = Optional.Create(channel), new RequestOptions { AuditLogReason = reason });
		}
		public static async Task ModifyGuildDefaultMsgNotifications(IGuild guild, DefaultMessageNotifications msgNotifs, string reason)
		{
			await guild.ModifyAsync(x => x.DefaultMessageNotifications = msgNotifs, new RequestOptions { AuditLogReason = reason });
		}
		public static async Task ModifyGuildVerificationLevel(IGuild guild, VerificationLevel verifLevel, string reason)
		{
			await guild.ModifyAsync(x => x.VerificationLevel = verifLevel, new RequestOptions { AuditLogReason = reason });
		}
		public static async Task ModifyGuildIcon(IGuild guild, FileInfo fileInfo, string reason)
		{
			using (var stream = new StreamReader(fileInfo.FullName))
			{
				await guild.ModifyAsync(x => x.Icon = new Image(stream.BaseStream), new RequestOptions { AuditLogReason = reason });
			}
		}
	}
}