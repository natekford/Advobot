using Advobot.Core.Classes.Attributes;
using Advobot.Core.Utilities;
using Discord;
using Discord.Commands;
using System;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Shorter way to write the used modulebase and also has every command go through the <see cref="CommandRequirementAttribute"/> first.
	/// </summary>
	[CommandRequirement]
	public abstract class NonSavingModuleBase : ModuleBase<AdvobotSocketCommandContext>
	{
		/// <summary>
		/// Gets a request options that mainly is used for the reason in the audit log.
		/// </summary>
		/// <param name="reason"></param>
		/// <returns></returns>
		public RequestOptions GetRequestOptions(string reason = "")
		{
			return String.IsNullOrWhiteSpace(reason)
				? ClientUtils.CreateRequestOptions($"Action by {Context.User.Format()}.")
				: ClientUtils.CreateRequestOptions($"Action by {Context.User.Format()}. Reason: {reason}.");
		}
	}
}