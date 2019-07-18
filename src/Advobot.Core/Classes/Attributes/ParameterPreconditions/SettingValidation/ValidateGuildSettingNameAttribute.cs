using Advobot.Classes.Modules;
using AdvorangesUtils;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Advobot.Classes.Attributes.ParameterPreconditions.SettingValidation
{
	/// <summary>
	/// Makes sure the passed in string is a valid guild setting name.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class ValidateGuildSettingNameAttribute : AdvobotParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(AdvobotCommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
		{
			return context.GuildSettings.GetSettingNames().CaseInsContains((string)value)
				? Task.FromResult(PreconditionResult.FromSuccess())
				: Task.FromResult(PreconditionResult.FromError("Invalid guild setting name supplied."));
		}
		/// <summary>
		/// Returns a string describing what this attribute requires.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> "Valid localized guild setting name";
	}
}
