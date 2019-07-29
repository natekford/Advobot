using System;
using System.Threading.Tasks;
using Advobot.Modules;
using Advobot.Services.HelpEntries;
using AdvorangesUtils;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Attributes.ParameterPreconditions.SettingValidation
{
	/// <summary>
	/// Makes sure the passed in string is a valid command category.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class ValidateCommandCategoryAttribute : AdvobotParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(IAdvobotCommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
		{
			var helpEntries = services.GetRequiredService<IHelpEntryService>();
			return helpEntries.GetCategories().CaseInsContains((string)value)
				? Task.FromResult(PreconditionResult.FromSuccess())
				: Task.FromResult(PreconditionResult.FromError("Invalid category supplied."));
		}
		/// <summary>
		/// Returns a string describing what this attribute requires.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> "Valid command category";
	}
}
