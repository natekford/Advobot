using System;
using System.Threading.Tasks;
using Advobot.Classes.Modules;
using Advobot.Services.BotSettings;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Classes.Attributes.Preconditions
{
	/// <summary>
	/// Requires trusted user status before this command will execute.
	/// </summary>
	[Obsolete("Remove this for safety reasons? Or let trusted users exist?")]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class RequireTrustedUserAttribute : AdvobotPreconditionAttribute
	{
		/// <inheritdoc />
		public override bool Visible => true;

		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(AdvobotCommandContext context, CommandInfo command, IServiceProvider services)
		{
			return services.GetRequiredService<IBotSettings>().TrustedUsers.Contains(context.User.Id)
				? Task.FromResult(PreconditionResult.FromSuccess())
				: Task.FromResult(PreconditionResult.FromError("User is not a trusted user."));
		}
		/// <summary>
		/// Returns a string describing what this attribute requires.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> "Trusted user";
	}
}