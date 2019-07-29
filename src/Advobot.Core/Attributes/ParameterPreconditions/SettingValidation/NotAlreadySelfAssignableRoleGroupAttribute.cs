using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Modules;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.SettingValidation
{
	/// <summary>
	/// Makes sure the passed in number isn't currently being used for a self assignable roles group.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class NotAlreadySelfAssignableRoleGroupAttribute : AdvobotParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(IAdvobotCommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
		{
			if (!(value is int num))
			{
				throw new ArgumentException(nameof(value));
			}
			return context.Settings.SelfAssignableGroups.Any(x => x.Group == num)
				? Task.FromResult(PreconditionResult.FromError($"The group number `{num}` is already being used."))
				: Task.FromResult(PreconditionResult.FromSuccess());
		}
		/// <summary>
		/// Returns a string describing what this attribute requires.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> "Group number not already being used";
	}
}
