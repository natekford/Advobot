using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes.Modules;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Classes.Attributes.ParameterPreconditions.SettingValidation
{
	/// <summary>
	/// Makes sure the passed in string is not already being used for a quote name.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class NotAlreadyQuoteNameAttribute : AdvobotParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(AdvobotCommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
		{
			if (!(value is string str))
			{
				throw new ArgumentException(nameof(value));
			}
			return context.GuildSettings.Quotes.Any(x => x.Name.CaseInsEquals(str))
				? Task.FromResult(PreconditionResult.FromError($"The quote name `{str}` is already being used."))
				: Task.FromResult(PreconditionResult.FromSuccess());
		}
		/// <summary>
		/// Returns a string describing what this attribute requires.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> "Not already used for a quote name";
	}
}
