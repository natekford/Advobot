using Advobot.Classes.Attributes;
using Advobot.Utilities;
using Discord;
using Discord.Commands;

namespace Advobot.Classes
{
	/// <summary>
	/// Shorter way to write the used modulebase and also has every command go through the <see cref="CommandRequirementAttribute"/> first.
	/// </summary>
	[CommandRequirement]
	[RequireContext(ContextType.Guild)]
	public abstract class AdvobotModuleBase : ModuleBase<AdvobotCommandContext>
	{
		/// <summary>
		/// Gets a request options that mainly is used for the reason in the audit log.
		/// </summary>
		/// <param name="reason"></param>
		/// <returns></returns>
		public RequestOptions GetRequestOptions(string reason = "")
		{
			return string.IsNullOrWhiteSpace(reason)
				? ClientUtils.CreateRequestOptions($"Action by {Context.User.Format()}.")
				: ClientUtils.CreateRequestOptions($"Action by {Context.User.Format()}. Reason: {reason}.");
		}
	}
}