using System;
using System.Threading.Tasks;

using Advobot.Services.HelpEntries;
using Advobot.Utilities;

using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions
{
	/// <summary>
	/// Makes sure that the passed in <see cref="IModuleHelpEntry"/> can be toggled.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class CanToggleAttribute
		: AdvobotParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string Summary => "Can be toggled";

		/// <inheritdoc />
		protected override Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			object value,
			IServiceProvider services)
		{
			if (!(value is IModuleHelpEntry entry))
			{
				return this.FromOnlySupports(typeof(IModuleHelpEntry)).Async();
			}
			else if (entry.AbleToBeToggled)
			{
				return PreconditionUtils.FromSuccess().Async();
			}
			return PreconditionUtils.FromError($"`{entry.Name}` cannot be toggled.").Async();
		}
	}
}