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
		protected override Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			object value,
			IServiceProvider services)
		{
			if (!(value is IModuleHelpEntry entry))
			{
				throw this.OnlySupports(typeof(IModuleHelpEntry));
			}
			else if (entry.AbleToBeToggled)
			{
				return PreconditionUtils.FromSuccessAsync();
			}
			return PreconditionUtils.FromErrorAsync($"`{entry.Name}` cannot be toggled.");
		}
	}
}
