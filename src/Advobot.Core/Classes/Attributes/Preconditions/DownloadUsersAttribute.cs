using System;
using System.Threading.Tasks;
using AdvorangesUtils;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Classes.Attributes.Preconditions
{
	/// <summary>
	/// Downloads all users before executing the command.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class DownloadUsersAttribute : PreconditionAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="DownloadUsersAttribute"/>.
		/// </summary>
		public DownloadUsersAttribute()
		{
			Group = nameof(DownloadUsersAttribute);
		}

		/// <summary>
		/// Makes sure every user is downloaded before running the command.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="command"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			if (!(context.Guild is SocketGuild socket) || !socket.HasAllMembers)
			{
				await context.Guild.DownloadUsersAsync().CAF();
			}
			return PreconditionResult.FromSuccess();
		}
	}
}