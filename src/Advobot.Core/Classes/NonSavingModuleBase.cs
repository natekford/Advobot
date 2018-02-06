using Advobot.Core.Classes.Attributes;
using Discord.Commands;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Shorter way to write the used modulebase and also has every command go through the <see cref="CommandRequirementAttribute"/> first.
	/// </summary>
	[CommandRequirement]
	public class NonSavingModuleBase : ModuleBase<AdvobotSocketCommandContext> { }
}