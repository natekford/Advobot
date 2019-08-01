using System;
using System.Threading.Tasks;
using Advobot.Services.GuildSettings;
using AdvorangesUtils;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Attributes.ParameterPreconditions.SettingValidation
{
	/// <summary>
	/// Makes sure the passed in string is a valid guild setting name.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class ValidateGuildSettingNameAttribute : AdvobotParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override async Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			object value,
			IServiceProvider services)
		{
			var settingsFactory = services.GetRequiredService<IGuildSettingsFactory>();
			var settings = await settingsFactory.GetOrCreateAsync(context.Guild).CAF();
			var settingNames = settings.GetSettingNames();
			if (settingNames.CaseInsContains((string)value))
			{
				return PreconditionResult.FromSuccess();
			}
			return PreconditionResult.FromError("Invalid guild setting name supplied.");
		}
		/// <summary>
		/// Returns a string describing what this attribute requires.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> "Valid localized guild setting name";
	}
}
