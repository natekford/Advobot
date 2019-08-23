using System;
using System.Threading.Tasks;
using Advobot.Services.BotSettings;
using Advobot.Utilities;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Attributes.Preconditions
{
	/// <summary>
	/// Requires trusted user status before this command will execute.
	/// </summary>
	[Obsolete("Remove this for safety reasons? Or let trusted users exist?")]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class RequireTrustedUserAttribute : PreconditionAttribute
	{
		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			CommandInfo command,
			IServiceProvider services)
		{
			var botSettings = services.GetRequiredService<IBotSettings>();
			if (botSettings.TrustedUsers.Contains(context.User.Id))
			{
				return PreconditionUtils.FromSuccessAsync();
			}
			return PreconditionUtils.FromErrorAsync("User is not a trusted user.");
		}
	}
}