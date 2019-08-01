using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Services.GuildSettings;
using AdvorangesUtils;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Attributes.ParameterPreconditions.SettingValidation
{
	/// <summary>
	/// Makes sure the passed in string is not already being used for a quote name.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class NotAlreadyQuoteNameAttribute : AdvobotParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override async Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			object value,
			IServiceProvider services)
		{
			if (!(value is string str))
			{
				throw new ArgumentException(nameof(value));
			}

			var settingsFactory = services.GetRequiredService<IGuildSettingsFactory>();
			var settings = await settingsFactory.GetOrCreateAsync(context.Guild).CAF();
			if (!settings.Quotes.Any(x => x.Name.CaseInsEquals(str)))
			{
				return PreconditionResult.FromSuccess();
			}
			return PreconditionResult.FromError($"The quote name `{str}` is already being used.");
		}
		/// <summary>
		/// Returns a string describing what this attribute requires.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> "Not already used for a quote name";
	}
}
