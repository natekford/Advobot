using System;
using System.Threading.Tasks;
using Advobot.Classes.Modules;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Attributes.Preconditions
{
	/// <summary>
	/// Downloads all users before executing the command.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class DownloadUsersAttribute : AdvobotPreconditionAttribute
	{
		/// <inheritdoc />
		public override bool Visible => false;

		/// <inheritdoc />
		public override async Task<PreconditionResult> CheckPermissionsAsync(AdvobotCommandContext context, CommandInfo command, IServiceProvider services)
		{
			if (!context.Guild.HasAllMembers)
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