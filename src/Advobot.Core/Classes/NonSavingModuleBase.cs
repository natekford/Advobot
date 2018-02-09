using Advobot.Core.Classes.Attributes;
using Advobot.Core.Utilities;
using Discord;
using Discord.Commands;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Shorter way to write the used modulebase and also has every command go through the <see cref="CommandRequirementAttribute"/> first.
	/// </summary>
	[CommandRequirement]
	public class NonSavingModuleBase : ModuleBase<AdvobotSocketCommandContext>
	{
		public RequestOptions GetRequestOptions(string reason = "")
		{
			return ClientUtils.CreateRequestOptions($"Action by {Context.User.Format()}. Reason: {reason}.");
		}
	}
}