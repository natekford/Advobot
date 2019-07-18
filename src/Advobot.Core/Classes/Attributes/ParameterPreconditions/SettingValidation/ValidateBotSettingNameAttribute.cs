using Advobot.Classes.Modules;
using Advobot.Interfaces;
using AdvorangesUtils;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Advobot.Classes.Attributes.ParameterPreconditions.SettingValidation
{
	/// <summary>
	/// Makes sure the passed in string is a valid bot setting name.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class ValidateBotSettingNameAttribute : AdvobotParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(AdvobotCommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
		{
			var helpEntries = services.GetRequiredService<IBotSettings>();
			return helpEntries.GetSettingNames().CaseInsContains((string)value)
				? Task.FromResult(PreconditionResult.FromSuccess())
				: Task.FromResult(PreconditionResult.FromError("Invalid bot setting name supplied."));
		}
		/// <summary>
		/// Returns a string describing what this attribute requires.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> "Valid localized bot setting name";
	}
}
