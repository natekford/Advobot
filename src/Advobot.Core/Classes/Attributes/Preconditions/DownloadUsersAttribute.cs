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
	public sealed class DownloadUsersAttribute : SelfGroupPreconditionAttribute
	{
		/// <inheritdoc />
		public override bool Visible => false;

		/// <inheritdoc />
		public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			if (!(context.Guild is SocketGuild socket) || !socket.HasAllMembers)
			{
				await context.Guild.DownloadUsersAsync().CAF();
			}
			return PreconditionResult.FromSuccess();
		}
		/// <summary>
		/// Returns a string describing what this attribute requires.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> "All users downloaded by bot";
	}
}