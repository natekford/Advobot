using System;
using System.Threading.Tasks;
using Advobot.Services.HelpEntries;
using Advobot.Utilities;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions
{
	/// <summary>
	/// Makes sure that the passed in <see cref="IHelpEntry"/> can be toggled.
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
			if (!(value is IHelpEntry entry))
			{
				throw this.OnlySupports(typeof(IHelpEntry));
			}
			else if (entry.AbleToBeToggled)
			{
				return this.FromSuccessAsync();
			}
			return this.FromErrorAsync($"`{entry.Name}` cannot be toggled.");
		}
	}
}
